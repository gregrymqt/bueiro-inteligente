package br.edu.fatecpg.feature.monitoring.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository

import br.edu.fatecpg.core.navigation.LocationHandler`n`nclass MonitoringViewModelFactory(private val repository: MonitoringRepository, private val locationHandler: LocationHandler) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(MonitoringViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return MonitoringViewModel(repository, locationHandler) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
