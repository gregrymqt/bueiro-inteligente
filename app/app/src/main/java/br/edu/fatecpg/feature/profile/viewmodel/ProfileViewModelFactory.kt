package br.edu.fatecpg.feature.profile.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.feature.profile.repository.ProfileRepository

class ProfileViewModelFactory(private val repository: ProfileRepository) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        try {
            if (modelClass.isAssignableFrom(ProfileViewModel::class.java)) {        
                Log.d("ProfileViewModelFactory", "Construindo o ProfileViewModel")
                @Suppress("UNCHECKED_CAST")
                return ProfileViewModel(repository) as T
            }
            Log.e("ProfileViewModelFactory", "Modelo de ViewModel invalido submetido à fabrica base de perfil: ${modelClass.name}")
            throw IllegalArgumentException("Unknown ViewModel class")
        } catch (e: Exception) {
            Log.e("ProfileViewModelFactory", "Queda fortuita do container de ViewModel -> Fabrica abortada.", e)
            throw e
        }
    }
}
