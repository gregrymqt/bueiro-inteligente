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
    data class Success(val data: DrainStatusDTO) : MonitoringUiState()
    data class Error(val message: String) : MonitoringUiState()
}

class MonitoringViewModel(private val repository: MonitoringRepository) : ViewModel() {

    private val _uiState = MutableStateFlow<MonitoringUiState>(MonitoringUiState.Loading)
    val uiState: StateFlow<MonitoringUiState> = _uiState.asStateFlow()

    fun fetchDrainStatus(id: String) {
        // Dispara a coroutine já no dispatcher correto com abstração de IO
        viewModelScope.launch(Dispatchers.IO) {
            _uiState.value = MonitoringUiState.Loading
            
            repository.getDrainStatus(id).fold(
                onSuccess = { drainStatus ->
                    _uiState.value = MonitoringUiState.Success(drainStatus)
                },
                onFailure = { error ->
                    _uiState.value = MonitoringUiState.Error(
                        error.message ?: "Erro desconhecido ao carregar status do bueiro"
                    )
                }
            )
        }
    }
}
