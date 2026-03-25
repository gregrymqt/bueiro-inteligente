package br.edu.fatecpg.feature.home.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class HomeViewModel(
    private val realtimeRepository: RealtimeRepository
) : ViewModel() {

    private val _activeAlert = MutableStateFlow<DrainStatusDTO?>(null)
    val activeAlert: StateFlow<DrainStatusDTO?> = _activeAlert.asStateFlow()

    init {
        viewModelScope.launch(Dispatchers.Main) {
            realtimeRepository.alertas.collect { status ->
                val currentStatus = status.status.lowercase()
                if (currentStatus == "alerta" || currentStatus == "crítico" || currentStatus == "critico") {
                    _activeAlert.value = status
                }
            }
        }
    }

    fun dismissAlert() {
        _activeAlert.value = null
    }
}