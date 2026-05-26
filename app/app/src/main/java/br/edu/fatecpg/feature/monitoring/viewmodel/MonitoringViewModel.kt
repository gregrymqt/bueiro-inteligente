package br.edu.fatecpg.feature.monitoring.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class MonitoringUiState {
    data object Loading : MonitoringUiState()
    data class Success(val drains: List<DrainStatusDTO>) : MonitoringUiState()  
    data class Error(val message: String) : MonitoringUiState()
}

class MonitoringViewModel(
    private val repository: MonitoringRepository, 
    private val locationHandler: LocationHandler,
    private val realtimeRepository: RealtimeRepository,
    private val tokenManager: TokenManager
) : ViewModel() {

    private val _uiState = MutableStateFlow<MonitoringUiState>(MonitoringUiState.Loading)
    val uiState: StateFlow<MonitoringUiState> = _uiState.asStateFlow()

    private var bueirosMemoria = listOf<DrainStatusDTO>()
    private val atualizacoesPendentes = mutableMapOf<String, DrainStatusDTO>()

    private val _showLoginDialog = MutableStateFlow(false)
    val showLoginDialog: StateFlow<Boolean> = _showLoginDialog.asStateFlow()

    private val _expandedDrainId = MutableStateFlow<String?>(null)
    val expandedDrainId: StateFlow<String?> = _expandedDrainId.asStateFlow()

    fun onDrainClick(isLoggedIn: Boolean, drain: DrainStatusDTO) {
        if (!isLoggedIn) {
            _showLoginDialog.value = true
            return
        }

        // Mantemos o id/hardwareId para controlar a expansão visual do card na tela
        val visualExpandedId = drain.id ?: drain.hardwareId
        val currentExpandedId = _expandedDrainId.value

        // Mas para o SignalR, usamos estritamente o hardwareId do bueiro físico
        val socketGroupId = drain.hardwareId

        if (currentExpandedId == visualExpandedId) {
            _expandedDrainId.value = null
            realtimeRepository.leaveDrain(socketGroupId) // Sai usando o HardwareId
            Log.d("MonitoringViewModel", "Saindo do canal Pub/Sub do hardware: $socketGroupId")
        } else {
            // Desinscreve do anterior se houver (buscando o hardwareId correspondente)
            if (currentExpandedId != null) {
                bueirosMemoria.find { (it.id ?: it.hardwareId) == currentExpandedId }?.let { bueiroAnterior ->
                    realtimeRepository.leaveDrain(bueiroAnterior.hardwareId)
                }
            }

            _expandedDrainId.value = visualExpandedId
            realtimeRepository.joinDrain(socketGroupId) // Entra usando o HardwareId
            Log.d("MonitoringViewModel", "Inscrito com sucesso no grupo de hardware: $socketGroupId")
        }
    }

    fun openDrainInMaps(drain: DrainStatusDTO) {
        val lat = drain.latitude
        val lng = drain.longitude
        val safeName = drain.name ?: "Bueiro Desconhecido"
        if (lat != null && lng != null) {
            Log.d("MonitoringViewModel", "Requisitando abertura de localizacao GPS do bueiro $safeName")
            locationHandler.openLocation(lat, lng, safeName)
        } else {
            Log.w("MonitoringViewModel", "Tentativa de abrir localizacao de bueiro que nao possui coordenadas. ID = ${drain.id ?: drain.hardwareId}")
        }
    }

    fun dismissLoginDialog() {
        _showLoginDialog.value = false
    }

    init {
        Log.d("MonitoringViewModel", "Inicializando tela de Monitoramento, buscando dados parciais...")

        // Inicia conexão WebSocket automaticamente
        realtimeRepository.connect(tokenManager.getToken())

        refreshDrains()

        // Coleta de alertas em tempo real contínua para evitar perda de pacotes durante o Loading
        viewModelScope.launch {
            realtimeRepository.alertas.collect { alerta ->

                // 1. Guarda no mapa de transição para o refreshDrains pegar se estiver carregando
                atualizacoesPendentes[alerta.hardwareId] = alerta

                // 2. Mapeia para dentro do bueirosMemoria de forma limpa e direta pelo HardwareId
                bueirosMemoria = bueirosMemoria.map { drain ->
                    if (drain.hardwareId.equals(alerta.hardwareId, ignoreCase = true)) {
                        alerta
                    } else {
                        drain
                    }
                }

                // 3. Se estiver em estado de Success, emite a nova lista imediatamente para acionar recomposição
                if (_uiState.value is MonitoringUiState.Success) {
                    _uiState.value = MonitoringUiState.Success(bueirosMemoria)
                }
            }
        }
    }

    fun refreshDrains() {
        _uiState.value = MonitoringUiState.Loading
        viewModelScope.launch {
            repository.getAllDrains()
                .onSuccess { drains ->
                    Log.i("MonitoringViewModel", "Trocando estado UI pra Success. Lista recebida: ${drains.size} bueiros")

                    // 💡 CORREÇÃO: Usa estritamente o hardwareId para ler o mapa de tempo real
                    bueirosMemoria = drains.map { drain ->
                        atualizacoesPendentes[drain.hardwareId] ?: drain
                    }

                    _uiState.value = MonitoringUiState.Success(bueirosMemoria)

                    // 🚀 PERFEITO: Agora sim o App entra na sala certa do SignalR!
                    drains.forEach { drain ->
                        realtimeRepository.joinDrain(drain.hardwareId)
                        Log.v("MonitoringViewModel", "Inscricao automatica realizada para o grupo: ${drain.hardwareId}")
                    }
                }
                .onFailure { error ->
                    Log.e("MonitoringViewModel", "Repasse de falha de carregamento: ${error.message}", error)
                    _uiState.value = MonitoringUiState.Error(
                        error.message ?: "Erro desconhecido ao carregar bueiros"
                    )
                }
        }
    }

    fun fetchDrainStatus(id: String) {
        viewModelScope.launch {
            _uiState.value = MonitoringUiState.Loading

            repository.getDrainStatus(id)
                .onSuccess {
                    Log.d("MonitoringViewModel", "Status fetch para id $id concluiu.")
                }
                .onFailure { error ->
                    Log.w("MonitoringViewModel", "Falha refletida na fetchDrainStatus: ${error.message}", error)
                    _uiState.value = MonitoringUiState.Error(
                        error.message ?: "Erro desconhecido ao carregar status do bueiro"
                    )
                }
        }
    }

    override fun onCleared() {
        super.onCleared()
        _expandedDrainId.value?.let { currentId ->
            bueirosMemoria.find { (it.id ?: it.hardwareId) == currentId }?.let { bueiro ->
                realtimeRepository.leaveDrain(bueiro.hardwareId)
            }
        }
    }

    companion object {
        fun getStatusColor(status: String?): Long {
            val safeStatus = status?.lowercase() ?: "desconhecido"
            return try {
                when (safeStatus) {
                    "ok", "normal", "bom" -> 0xFF4CAF50 // Verde
                    "alerta", "warning" -> 0xFFFF9800 // Laranja
                    "crítico", "critico", "critical" -> 0xFFF44336 // Vermelho
                    else -> 0xFF9E9E9E // Cinza (Desconhecido)
                }
            } catch (e: Exception) {
                Log.e("MonitoringViewModel", "Erro ao parsear cor de status: $status", e)
                0xFF9E9E9E
            }
        }
    }
}
