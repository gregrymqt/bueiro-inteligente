package br.edu.fatecpg.feature.monitoring.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
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

class MonitoringViewModel(private val repository: MonitoringRepository) : ViewModel() {

    private val _uiState = MutableStateFlow<MonitoringUiState>(MonitoringUiState.Loading)
    val uiState: StateFlow<MonitoringUiState> = _uiState.asStateFlow()

    init {
        refreshDrains()
    }

    fun refreshDrains() {
        _uiState.value = MonitoringUiState.Loading
        viewModelScope.launch {
            repository.getAllDrains()
                .onSuccess { drains ->
                    _uiState.value = MonitoringUiState.Success(drains)
                }
                .onFailure { error ->
                    _uiState.value = MonitoringUiState.Error(
                        error.message ?: "Erro desconhecido ao carregar bueiros"
                    )
                }
        }
    }

    fun fetchDrainStatus(id: String) {
        // Dispara a coroutine já no dispatcher correto com abstração de IO
        viewModelScope.launch(Dispatchers.IO) {
            _uiState.value = MonitoringUiState.Loading
            
            repository.getDrainStatus(id)
                .onSuccess { drainStatus ->
                    // Tratamento provisório caso ainda precise buscar por id
                    val currentDrains = (_uiState.value as? MonitoringUiState.Success)?.drains ?: emptyList()
                    // _uiState.value = MonitoringUiState.Success(listOf(drainStatus)) // Se quisesse sobrescrever
                }
                .onFailure { error ->
                    _uiState.value = MonitoringUiState.Error(
                        error.message ?: "Erro desconhecido ao carregar status do bueiro"
                    )
                }
        }
    }

    companion object {
        /**
         * Retorna uma cor em formato Long (podendo ser convertida em Compose Color via Color(value)
         * ou XML via android.graphics.Color) com base no status do bueiro.
         */
        fun getStatusColor(status: String): Long {
            return when (status.lowercase()) {
                "ok" -> 0xFF4CAF50 // Verde
                "alerta" -> 0xFFFF9800 // Laranja
                "crítico", "critico" -> 0xFFF44336 // Vermelho
                else -> 0xFF9E9E9E // Cinza (Desconhecido)
            }
        }
    }
}
