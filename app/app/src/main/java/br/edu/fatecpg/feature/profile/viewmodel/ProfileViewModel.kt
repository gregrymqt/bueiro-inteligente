package br.edu.fatecpg.feature.profile.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.feature.profile.dto.UserDTO
import br.edu.fatecpg.feature.profile.repository.ProfileRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class ProfileUiState {
    object Idle : ProfileUiState()
    object Loading : ProfileUiState()
    data class Success(val user: UserDTO) : ProfileUiState()
    data class Error(val message: String) : ProfileUiState()
}

class ProfileViewModel(private val repository: ProfileRepository) : ViewModel() {

    private val _uiState = MutableStateFlow<ProfileUiState>(ProfileUiState.Idle)
    val uiState: StateFlow<ProfileUiState> = _uiState.asStateFlow()

    fun loadProfile() {
        try {
            Log.d("ProfileViewModel", "Iniciando carregamento do perfil via coroutineScope")
            viewModelScope.launch {
                try {
                    _uiState.value = ProfileUiState.Loading
                    repository.fetchUserProfile()
                        .onSuccess { user ->
                            Log.i("ProfileViewModel", "Perfil renderizavel carregado! Trocando state para Success.")
                            _uiState.value = ProfileUiState.Success(user)
                        }
                        .onFailure { error ->
                            Log.w("ProfileViewModel", "Estado de falha disparado no repositorio de viewmodel. Rastreio:", error)
                            _uiState.value = ProfileUiState.Error(error.message ?: "Erro desconhecido ao carregar perfil")
                        }
                } catch (e: Exception) {
                    Log.e("ProfileViewModel", "A coroutine do loadProfile encontrou um catch fatal", e)
                    _uiState.value = ProfileUiState.Error("Erro inesperado ao gerar tela de perfil.")
                }
            }
        } catch (e: Exception) {
            Log.e("ProfileViewModel", "O dispatcher de carregamento do perfil desabou", e)
            _uiState.value = ProfileUiState.Error("Falha na renderização assíncrona.")
        }
    }
}
