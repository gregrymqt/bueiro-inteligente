package br.edu.fatecpg.feature.auth.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.feature.auth.repository.AuthRepository

class LoginViewModelFactory(private val repository: AuthRepository) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        try {
            if (modelClass.isAssignableFrom(LoginViewModel::class.java)) {
                Log.d("LoginViewModelFactory", "Criando instância de LoginViewModel")
                @Suppress("UNCHECKED_CAST")
                return LoginViewModel(repository) as T
            }
            Log.e("LoginViewModelFactory", "Tentativa de criar ViewModel desconhecido: ")
            throw IllegalArgumentException("Unknown ViewModel class")
        } catch (e: Exception) {
            Log.e("LoginViewModelFactory", "Erro ao criar ViewModel em LoginViewModelFactory", e)
            throw e
        }
    }
}
