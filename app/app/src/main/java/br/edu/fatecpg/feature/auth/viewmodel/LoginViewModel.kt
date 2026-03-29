package br.edu.fatecpg.feature.auth.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.auth.dto.LoginRequest
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class LoginUiState {
    object Idle : LoginUiState()
    object Loading : LoginUiState()
    object Success : LoginUiState()
    data class Error(val message: String) : LoginUiState()
}

class LoginViewModel(
    private val repository: AuthRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoginUiState>(LoginUiState.Idle)    
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    fun performLogin(email: String, password: String) {
        try {
            if (email.isBlank() || password.isBlank()) {
                Log.w("LoginViewModel", "Tentativa de login com campos vazios.")
                _uiState.value = LoginUiState.Error("Preencha todos os campos para continuar.")
                return
            }

            _uiState.value = LoginUiState.Loading

            viewModelScope.launch {
                try {
                    val result = repository.login(LoginRequest(email, password))        

                    result.onSuccess {
                        Log.i("LoginViewModel", "Recebido status de sucesso vindo do repositorio para login.")
                        _uiState.value = LoginUiState.Success
                    }.onFailure { exception ->
                        Log.w("LoginViewModel", "Falha relata pelo login via banco de dados: ${exception.message}")
                        _uiState.value = LoginUiState.Error(exception.message ?: "Ocorreu um erro desconhecido.")
                    }
                } catch (e: Exception) {
                    Log.e("LoginViewModel", "Erro grave ocorrido durante a Coroutine de performação do login", e)
                    _uiState.value = LoginUiState.Error("Erro inesperado durante a tentativa de login.")
                }
            }
        } catch (e: Exception) {
            Log.e("LoginViewModel", "Erro inesperado ao perfomar chamada principal de login", e)
            _uiState.value = LoginUiState.Error("Erro interno no aplicativo.")
        }
    }

    fun resetState() {
        if (_uiState.value !is LoginUiState.Idle) {
            _uiState.value = LoginUiState.Idle
        }
    }
}
