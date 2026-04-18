package br.edu.fatecpg.feature.auth.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.auth.dto.RegisterRequest
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class RegisterUiState {
    object Idle : RegisterUiState()
    object Loading : RegisterUiState()
    object Success : RegisterUiState()
    data class Error(val message: String) : RegisterUiState()
}

class RegisterViewModel(private val repository: AuthRepository) : ViewModel() { 

    private val emailRegex = Regex("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$")
    private val passwordRegex = Regex("^(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9\\s]).{8,}$")

    private val _uiState = MutableStateFlow<RegisterUiState>(RegisterUiState.Idle)
    val uiState: StateFlow<RegisterUiState> = _uiState.asStateFlow()

    fun performRegister(email: String, password: String, fullName: String) {
        try {
            if (email.isBlank() || password.isBlank() || fullName.isBlank()) {
                Log.w("RegisterViewModel", "Tentativa de registro negada: campos vazios.")
                _uiState.value = RegisterUiState.Error("Todos os campos devem ser preenchidos.")
                return
            }

            if (!isEmailValid(email.trim())) {
                Log.w("RegisterViewModel", "Tentativa de registro negada: e-mail invalido.")
                _uiState.value = RegisterUiState.Error("Insira um e-mail válido.")
                return
            }

            if (!isPasswordStrong(password)) {
                Log.w("RegisterViewModel", "Tentativa de registro negada: senha fraca.")
                _uiState.value = RegisterUiState.Error("A senha deve ter 8+ caracteres, incluindo maiúsculas, números e símbolos.")
                return
            }

            viewModelScope.launch {
                try {
                    _uiState.value = RegisterUiState.Loading

                    val request = RegisterRequest(email = email.trim(), password = password, fullName = fullName)
                    val result = repository.register(request)

                    result.onSuccess {
                        Log.i("RegisterViewModel", "Registro reportado com sucesso pelo reposit�rio.")
                        _uiState.value = RegisterUiState.Success
                    }.onFailure { exception ->
                        Log.w("RegisterViewModel", "Falha reportada no registro de db: ${exception.message}")
                        _uiState.value = RegisterUiState.Error(exception.message ?: "Erro desconhecido ao cadastrar.")
                    }
                } catch (e: Exception) {
                    Log.e("RegisterViewModel", "Erro cr�tico capturado na coroutine de performa��o do registro", e)
                    _uiState.value = RegisterUiState.Error("Erro inesperado durante a tentativa de registro.")
                }
            }
        } catch (e: Exception) {
            Log.e("RegisterViewModel", "Erro critico na camada do ViewModel ao processar click da view de registro", e)
            _uiState.value = RegisterUiState.Error("Erro interno no aplicativo.")
        }
    }

    private fun isEmailValid(email: String): Boolean {
        return email.matches(emailRegex)
    }

    private fun isPasswordStrong(password: String): Boolean {
        return password.matches(passwordRegex)
    }

    fun resetState() {
        _uiState.value = RegisterUiState.Idle
    }
}
