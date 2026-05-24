package br.edu.fatecpg.feature.monitoring.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.core.navigation.LocationHandler
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
    private val realtimeRepository: RealtimeRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<MonitoringUiState>(MonitoringUiState.Loading)
    val uiState: StateFlow<MonitoringUiState> = _uiState.asStateFlow()

    private val _showLoginDialog = MutableStateFlow(false)
    val showLoginDialog: StateFlow<Boolean> = _showLoginDialog.asStateFlow()

    private val _expandedDrainId = MutableStateFlow<String?>(null)
    val expandedDrainId: StateFlow<String?> = _expandedDrainId.asStateFlow()

    fun onDrainClick(isLoggedIn: Boolean, drain: DrainStatusDTO) {
        if (!isLoggedIn) {
            Log.d("MonitoringViewModel", "Usuario nao autenticado tentou acessar detalhe de bueiro. Mostrando modal de login.")
            _showLoginDialog.value = true
            return
        }

        val currentExpandedId = _expandedDrainId.value
        
        if (currentExpandedId == drain.id) {
            // Se o bueiro clicado já for o expandido, fecha e desinscreve
            _expandedDrainId.value = null
            realtimeRepository.leaveDrain(drain.id)
            Log.d("MonitoringViewModel", "Recolhendo bueiro e saindo do canal pub/sub: ${drain.id}")
        } else {
            // Se for um bueiro novo
            // 1. Desinscreve do anterior se houver
            currentExpandedId?.let { 
                realtimeRepository.leaveDrain(it)
                Log.d("MonitoringViewModel", "Saindo do canal anterior: $it")
            }
            
            // 2. Atualiza para o novo ID e se inscreve
            _expandedDrainId.value = drain.id
            realtimeRepository.joinDrain(drain.id)
            Log.d("MonitoringViewModel", "Expandindo bueiro e entrando no canal pub/sub: ${drain.id}")
        }
    }

    fun openDrainInMaps(drain: DrainStatusDTO) {
        val lat = drain.latitude
        val lng = drain.longitude
        if (lat != null && lng != null) {
            Log.d("MonitoringViewModel", "Requisitando abertura de localizacao GPS do bueiro ${drain.name}")
            locationHandler.openLocation(lat, lng, drain.name)
        } else {
            Log.w("MonitoringViewModel", "Tentativa de abrir localizacao de bueiro que nao possui coordenadas. ID = ${drain.id}")
        }
    }

    fun dismissLoginDialog() {
        _showLoginDialog.value = false
    }

    init {
        Log.d("MonitoringViewModel", "Inicializando tela de Monitoramento, buscando dados parciais...")
        refreshDrains()
        
        // Coleta de alertas em tempo real
        viewModelScope.launch {
            realtimeRepository.alertas.collect { alerta ->
                val currentState = _uiState.value
                val expandedId = _expandedDrainId.value

                if (currentState is MonitoringUiState.Success && expandedId != null) {
                    // Comparação robusta por ID ou HardwareId (Case Insensitive e Safe)
                    val isTargetDrain = alerta.id.equals(expandedId, ignoreCase = true) || 
                                       alerta.hardwareId.equals(expandedId, ignoreCase = true)

                    if (isTargetDrain) {
                        Log.d("MonitoringViewModel", "Recebido alerta RT compatível: ${alerta.id}. Atualizando lista.")
                        val updatedDrains = currentState.drains.map { drain ->
                            // Atualiza o item específico na lista se bater com ID ou HardwareId
                            if (drain.id.equals(alerta.id, ignoreCase = true) || 
                                drain.hardwareId.equals(alerta.hardwareId, ignoreCase = true)) {
                                alerta
                            } else {
                                drain
                            }
                        }
                        _uiState.value = MonitoringUiState.Success(updatedDrains)
                    }
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
                    _uiState.value = MonitoringUiState.Success(drains)
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
        _expandedDrainId.value?.let { id ->
            Log.d("MonitoringViewModel", "ViewModel destruida. Saindo do canal pub/sub para bueiro: $id")
            realtimeRepository.leaveDrain(id)
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
