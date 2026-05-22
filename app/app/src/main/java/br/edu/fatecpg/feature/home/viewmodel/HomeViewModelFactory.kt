package br.edu.fatecpg.feature.home.viewmodel

import android.content.Context
import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.core.notifications.NotificationHelper
import br.edu.fatecpg.feature.device.repository.DeviceRepository
import br.edu.fatecpg.feature.home.repository.HomeRepository
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository

class HomeViewModelFactory(
    private val context: Context,
    private val realtimeRepository: RealtimeRepository,
    private val homeRepository: HomeRepository,
    private val deviceRepository: DeviceRepository,
    private val tokenManager: TokenManager
) : ViewModelProvider.Factory {

    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        try {
            if (modelClass.isAssignableFrom(HomeViewModel::class.java)) {
                Log.d("HomeViewModelFactory", "Criando instância de HomeViewModel com suporte a Realtime e Notificações")
                
                val notificationHelper = NotificationHelper(context)
                
                @Suppress("UNCHECKED_CAST")
                return HomeViewModel(
                    realtimeRepository = realtimeRepository,
                    homeRepository = homeRepository,
                    deviceRepository = deviceRepository,
                    tokenManager = tokenManager,
                    notificationHelper = notificationHelper
                ) as T
            }
            Log.e("HomeViewModelFactory", "ViewModel desconhecido solicitado: ${modelClass.name}")
            throw IllegalArgumentException("Unknown ViewModel class")
        } catch (e: Exception) {
            Log.e("HomeViewModelFactory", "Erro ao criar HomeViewModel", e)
            throw e
        }
    }
}
