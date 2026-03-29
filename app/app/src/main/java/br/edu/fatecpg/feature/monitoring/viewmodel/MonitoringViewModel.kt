package br.edu.fatecpg.feature.monitoring.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository        
import br.edu.fatecpg.core.navigation.LocationHandler
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class MonitoringUiState {
    object Loading : MonitoringUiState()
    data class Success(val drains: List<DrainStatusDTO>) : MonitoringUiState()  
    data class Error(val message: String) : MonitoringUiState()
}

class MonitoringViewModel(private val repository: MonitoringRepository, private val locationHandler: LocationHandler) : ViewModel() {

    private val _uiState = MutableStateFlow<MonitoringUiState>(MonitoringUiState.Loading)
    val uiState: StateFlow<MonitoringUiState> = _uiState.asStateFlow()

    private val _showLoginDialog = MutableStateFlow(false)
    val showLoginDialog: StateFlow<Boolean> = _showLoginDialog.asStateFlow()    

    fun onDrainClick(isLoggedIn: Boolean, drain: DrainStatusDTO) {
        try {
            if (isLoggedIn) {
                val lat = drain.latitude
                val lng = drain.longitude
                if (lat != null && lng != null) {
                    Log.d("MonitoringViewModel", "Requisitando abertura de localizacao GPS do bueiro ${drain.idBueiro}")
                    locationHandler.openLocation(lat, lng, "Bueiro ${drain.idBueiro}")
                } else {
                    Log.w("MonitoringViewModel", "Tentativa de abrir localizacao de bueiro que nao possui coordenadas. ID = ${drain.idBueiro}")
                }
            } else {
                Log.d("MonitoringViewModel", "Usuario nao autenticado tentou acessar detalhe de bueiro. Mostrando modal de login.")
                _showLoginDialog.value = true
            }
        } catch (e: Exception) {
            Log.e("MonitoringViewModel", "Falha critica no evento de click do cado bueiro", e)
        }
    }

    fun dismissLoginDialog() {
        try {
            _showLoginDialog.value = false
        } catch (e: Exception) {
            Log.e("MonitoringViewModel", "Erro ao dispensar dialogo de erro de visualizacao reclusa", e)
        }
    }

    init {
        try {
            Log.d("MonitoringViewModel", "Inicializando tela de Monitoramento, buscando dados parciais...")
            refreshDrains()
        } catch (e: Exception) {
            Log.e("MonitoringViewModel", "Erro na inicialização", e)
        }
    }

    fun refreshDrains() {
        try {
            _uiState.value = MonitoringUiState.Loading
            viewModelScope.launch {
                try {
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
                } catch (e: Exception) {
                    Log.e("MonitoringViewModel", "Flow corrompido durante sub-rotina de rede no viewmodel (refreshDrains)", e)
                    _uiState.value = MonitoringUiState.Error("Ocorreu uma exceção crítica ao buscar os bueiros.")
                }
            }
        } catch (e: Exception) {
            Log.e("MonitoringViewModel", "Falha global ao emitir sinal visual (refreshDrains)", e)
        }
    }

    fun fetchDrainStatus(id: String) {
        try {
            // Dispara a coroutine já no dispatcher correto com abstração de IO  
            viewModelScope.launch(Dispatchers.IO) {
                try {
                    _uiState.value = MonitoringUiState.Loading

                    repository.getDrainStatus(id)
                        .onSuccess { drainStatus ->
                            // Tratamento provisório caso ainda precise buscar por id
                            val currentDrains = (_uiState.value as? MonitoringUiState.Success)?.drains ?: emptyList()
                            Log.d("MonitoringViewModel", "Status fetch para id $id concluiu. Refletindo logica custom (Nao implementada completamente aqui mas recebida ok)")
                            // _uiState.value = MonitoringUiState.Success(listOf(drainStatus)) // Se quisesse sobrescrever
                        }
                        .onFailure { error ->
                            Log.w("MonitoringViewModel", "Falha refletida na fetchDrainStatus: ${error.message}", error)
                            _uiState.value = MonitoringUiState.Error(
                                error.message ?: "Erro desconhecido ao carregar status do bueiro"
                            )
                        }
                } catch (e: Exception) {
                    Log.e("MonitoringViewModel", "Falha capturada no dispatcher bg para id unificado ($id)", e)
                    _uiState.value = MonitoringUiState.Error("Ocorreu uma quebra no carregamento de ID exato.")
                }
            }
        } catch (e: Exception) {
            Log.e("MonitoringViewModel", "Erro do motor de thread no fetchDrainStatus()", e)
        }
    }

    companion object {
        fun getStatusColor(status: String): Long {
            return try {
                when (status.lowercase()) {
                    "ok" -> 0xFF4CAF50 // Verde
                    "alerta" -> 0xFFFF9800 // Laranja
                    "crítico", "critico" -> 0xFFF44336 // Vermelho
                    else -> 0xFF9E9E9E // Cinza (Desconhecido)
                }
            } catch (e: Exception) {
                Log.e("MonitoringViewModel", "Erro ao parsear cor de status: $status", e)
                0xFF9E9E9E
            }
        }
    }
}
