package br.edu.fatecpg.feature.auth.viewmodel

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
        if (email.isBlank() || password.isBlank()) {
            _uiState.value = LoginUiState.Error("Preencha todos os campos para continuar.")
            return
        }

        _uiState.value = LoginUiState.Loading

        viewModelScope.launch {
            val result = repository.login(LoginRequest(email, password))
            
            result.onSuccess {
                _uiState.value = LoginUiState.Success
            }.onFailure { exception ->
                _uiState.value = LoginUiState.Error(exception.message ?: "Ocorreu um erro desconhecido.")
            }
        }
    }

    fun resetState() {
        if (_uiState.value !is LoginUiState.Idle) {
            _uiState.value = LoginUiState.Idle
        }
    }
}