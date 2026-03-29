package br.edu.fatecpg.feature.home.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository

class HomeViewModelFactory(
    private val realtimeRepository: RealtimeRepository,
    private val tokenManager: TokenManager
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        try {
            if (modelClass.isAssignableFrom(HomeViewModel::class.java)) {
                Log.d("HomeViewModelFactory", "Criando instância de HomeViewModel")
                @Suppress("UNCHECKED_CAST")
                return HomeViewModel(realtimeRepository, tokenManager) as T
            }
            Log.e("HomeViewModelFactory", "ViewModel desconhecido solicitado: ${modelClass.name}")
            throw IllegalArgumentException("Unknown ViewModel class")
        } catch (e: Exception) {
            Log.e("HomeViewModelFactory", "Erro ao criar ViewModel", e)
            throw e
        }
    }
}
