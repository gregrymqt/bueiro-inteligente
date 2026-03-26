package br.edu.fatecpg.feature.monitoring.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository

class MonitoringViewModelFactory(private val repository: MonitoringRepository) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(MonitoringViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return MonitoringViewModel(repository) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}