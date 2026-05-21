package br.edu.fatecpg.feature.monitoring.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.core.navigation.LocationHandler
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class MonitoringUiState {
    data object Loading : MonitoringUiState()
    data class Success(val drains: List<DrainStatusDTO>) : MonitoringUiState()  
    data class Error(val message: String) : MonitoringUiState()
}

class MonitoringViewModel(private val repository: MonitoringRepository, private val locationHandler: LocationHandler) : ViewModel() {

    private val _uiState = MutableStateFlow<MonitoringUiState>(MonitoringUiState.Loading)
    val uiState: StateFlow<MonitoringUiState> = _uiState.asStateFlow()

    private val _showLoginDialog = MutableStateFlow(false)
    val showLoginDialog: StateFlow<Boolean> = _showLoginDialog.asStateFlow()    

    fun onDrainClick(isLoggedIn: Boolean, drain: DrainStatusDTO) {
        if (isLoggedIn) {
            val lat = drain.latitude
            val lng = drain.longitude
            if (lat != null && lng != null) {
                Log.d("MonitoringViewModel", "Requisitando abertura de localizacao GPS do bueiro ${drain.name}")
                locationHandler.openLocation(lat, lng, drain.name)
            } else {
                Log.w("MonitoringViewModel", "Tentativa de abrir localizacao de bueiro que nao possui coordenadas. ID = ${drain.id}")
            }
        } else {
            Log.d("MonitoringViewModel", "Usuario nao autenticado tentou acessar detalhe de bueiro. Mostrando modal de login.")
            _showLoginDialog.value = true
        }
    }

    fun dismissLoginDialog() {
        _showLoginDialog.value = false
    }

    init {
        Log.d("MonitoringViewModel", "Inicializando tela de Monitoramento, buscando dados parciais...")
        refreshDrains()
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
