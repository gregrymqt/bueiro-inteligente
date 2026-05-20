package br.edu.fatecpg.feature.profile.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.feature.auth.repository.AuthRepository

class ProfileViewModelFactory(
    private val repository: AuthRepository, // Ajustado para repassar a dependência unificada
    private val locationHandler: LocationHandler,
    private val dashboardWebUrl: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        try {
            if (modelClass.isAssignableFrom(ProfileViewModel::class.java)) {
                Log.d("ProfileViewModelFactory", "Construindo o ProfileViewModel com escopo de Auth")
                @Suppress("UNCHECKED_CAST")
                return ProfileViewModel(
                    repository = repository,
                    locationHandler = locationHandler,
                    dashboardWebUrl = dashboardWebUrl
                ) as T
            }
            Log.e("ProfileViewModelFactory", "Modelo de ViewModel invalido submetido à fábrica: ${modelClass.name}")
            throw IllegalArgumentException("Unknown ViewModel class")
        } catch (e: Exception) {
            Log.e("ProfileViewModelFactory", "Falha crítica na injeção do contêiner de Perfil -> Abortado.", e)
            throw e
        }
    }
}