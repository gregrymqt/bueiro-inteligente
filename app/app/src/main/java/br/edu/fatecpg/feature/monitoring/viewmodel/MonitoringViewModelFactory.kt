package br.edu.fatecpg.feature.monitoring.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository        
import br.edu.fatecpg.core.navigation.LocationHandler

class MonitoringViewModelFactory(private val repository: MonitoringRepository, private val locationHandler: LocationHandler) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        try {
            if (modelClass.isAssignableFrom(MonitoringViewModel::class.java)) {     
                Log.d("MonitoringViewModelFactory", "Criando instância de MonitoringViewModel")
                @Suppress("UNCHECKED_CAST")
                return MonitoringViewModel(repository, locationHandler) as T        
            }
            Log.e("MonitoringViewModelFactory", "ViewModel desconhecido: ${modelClass.name}")
            throw IllegalArgumentException("Unknown ViewModel class")
        } catch (e: Exception) {
            Log.e("MonitoringViewModelFactory", "Erro forte na construcao da MonitoringViewModel", e)
            throw e
        }
    }
}
