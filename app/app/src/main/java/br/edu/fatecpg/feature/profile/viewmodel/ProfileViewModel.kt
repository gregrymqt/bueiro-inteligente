package br.edu.fatecpg.feature.profile.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.feature.auth.dto.UserDTO
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class ProfileUiState {
    data object Idle : ProfileUiState()
    data object Loading : ProfileUiState()
    data class Success(val user: UserDTO) : ProfileUiState()
    data class Error(val message: String) : ProfileUiState()
}

class ProfileViewModel(
    private val repository: AuthRepository, // Alterado para usar o repositório central de autenticação
    private val locationHandler: LocationHandler,
    private val dashboardWebUrl: String
) : ViewModel() {

    private val _uiState = MutableStateFlow<ProfileUiState>(ProfileUiState.Idle)
    val uiState: StateFlow<ProfileUiState> = _uiState.asStateFlow()

    private val _showLogoutDialog = MutableStateFlow(false)
    val showLogoutDialog: StateFlow<Boolean> = _showLogoutDialog.asStateFlow()

    val canOpenDashboardWeb: Boolean
        get() = dashboardWebUrl.isNotBlank()

    fun onAction(action: ProfileAction) {
        when (action) {
            ProfileAction.LoadProfile -> loadProfile()
            ProfileAction.OpenDashboardWeb -> openDashboardWeb()
        }
    }

    fun showLogoutConfirmation() {
        _showLogoutDialog.value = true
    }

    fun dismissLogoutConfirmation() {
        _showLogoutDialog.value = false
    }

    private fun loadProfile() {
        Log.d("ProfileViewModel", "Iniciando carregamento do perfil de usuário através da API de Auth")
        viewModelScope.launch {
            _uiState.value = ProfileUiState.Loading
            // Bate na rota correta: api/v1/auth/users/me
            repository.getCurrentUser()
                .onSuccess { user ->
                    Log.i("ProfileViewModel", "Perfil do usuário carregado com sucesso!")
                    _uiState.value = ProfileUiState.Success(user)
                }
                .onFailure { error ->
                    Log.w("ProfileViewModel", "Falha ao obter dados do perfil logado:", error)
                    _uiState.value = ProfileUiState.Error(error.message ?: "Erro desconhecido ao carregar perfil")
                }
        }
    }

    private fun openDashboardWeb() {
        if (dashboardWebUrl.isBlank()) {
            Log.w("ProfileViewModel", "Dashboard web URL nao configurada, ignorando acao externa")
            return
        }

        Log.d("ProfileViewModel", "Solicitando abertura do dashboard web: $dashboardWebUrl")
        locationHandler.openWebUrl(dashboardWebUrl)
    }
}