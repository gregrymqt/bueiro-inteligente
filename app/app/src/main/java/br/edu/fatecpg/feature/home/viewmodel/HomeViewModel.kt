package br.edu.fatecpg.feature.home.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.core.notifications.NotificationHelper
import br.edu.fatecpg.feature.device.repository.DeviceRepository
import br.edu.fatecpg.feature.home.dto.HomeResponseDTO
import br.edu.fatecpg.feature.home.repository.HomeRepository
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import com.google.firebase.messaging.FirebaseMessaging
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

// Estado reativo isolado para o conteúdo da Home
sealed class HomeUiState {
    object Idle : HomeUiState()
    object Loading : HomeUiState()
    data class Success(val data: HomeResponseDTO) : HomeUiState()
    data class Error(val message: String) : HomeUiState()
}

class HomeViewModel(
    private val realtimeRepository: RealtimeRepository,
    private val homeRepository: HomeRepository,
    private val deviceRepository: DeviceRepository,
    private val tokenManager: TokenManager,
    private val notificationHelper: NotificationHelper
) : ViewModel() {

    // --- ESTADOS REATIVOS ---
    private val _uiState = MutableStateFlow<HomeUiState>(HomeUiState.Idle)
    val uiState: StateFlow<HomeUiState> = _uiState.asStateFlow()

    private val _activeAlert = MutableStateFlow<DrainStatusDTO?>(null)
    val activeAlert: StateFlow<DrainStatusDTO?> = _activeAlert.asStateFlow()

    private val _connectionError = MutableStateFlow<String?>(null)
    val connectionError: StateFlow<String?> = _connectionError.asStateFlow()

    init {
        try {
            Log.d("HomeViewModel", "Inicializando HomeViewModel. Conectando WebSocket e buscando dados.")
            realtimeRepository.connect(tokenManager.getToken())
            loadHomeContent() // <-- Dispara o carregamento dos Stat Cards e Carousels
            syncDeviceToken()
        } catch (e: Exception) {
            Log.e("HomeViewModel", "Erro inicial no HomeViewModel", e)
        }

        // Observa Alertas Críticos do WebSocket
        viewModelScope.launch(Dispatchers.Main) {
            try {
                realtimeRepository.alertas.collect { status ->
                    try {
                        val currentStatus = status.status?.lowercase() ?: ""
                        if (currentStatus == "alerta" || currentStatus == "crítico" || currentStatus == "critico") {
                            Log.i("HomeViewModel", "Alerta recebido para o bueiro: ${status.name}")
                            _activeAlert.value = status

                            // Dispara notificação nativa para status críticos
                            if (currentStatus == "crítico" || currentStatus == "critico") {
                                notificationHelper.showCriticalNotification(status)
                            }
                        }
                    } catch (e: Exception) {
                        Log.w("HomeViewModel", "Erro ao checar status do alerta", e)
                    }
                }
            } catch (e: Exception) {
                Log.e("HomeViewModel", "Erro crítico ao processar fluxo de alertas", e)
            }
        }

        // Observa Erros de Conexão do WebSocket
        viewModelScope.launch(Dispatchers.Main) {
            try {
                realtimeRepository.connectionError.collect { error ->
                    if (error != null) {
                        Log.w("HomeViewModel", "Erro de conexao reportado: $error")
                    }
                    _connectionError.value = error
                }
            } catch (e: Exception) {
                Log.e("HomeViewModel", "Erro ao processar fluxo de erros de conexao", e)
            }
        }
    }

    /**
     * Carrega os dados administrativos/públicos da Home com gestão de estado de UI.
     */
    fun loadHomeContent() {
        Log.d("HomeViewModel", "Iniciando carregamento do conteúdo HTTP da Home")
        viewModelScope.launch {
            _uiState.value = HomeUiState.Loading
            homeRepository.getHomeContent()
                .onSuccess { response ->
                    Log.i("HomeViewModel", "Conteúdo da Home carregado com sucesso!")
                    _uiState.value = HomeUiState.Success(response)
                }
                .onFailure { error ->
                    Log.w("HomeViewModel", "Falha ao carregar conteúdo da Home", error)
                    _uiState.value = HomeUiState.Error(
                        error.message ?: "Não foi possível carregar as estatísticas do sistema."
                    )
                }
        }
    }

    private fun syncDeviceToken() {
        if (!tokenManager.getToken().isNullOrEmpty()) {
            FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
                if (task.isSuccessful) {
                    val token = task.result
                    viewModelScope.launch(Dispatchers.IO) {
                        try {
                            deviceRepository.registerToken(token)
                            Log.i("HomeViewModel", "Token FCM sincronizado com sucesso")
                        } catch (e: Exception) {
                            Log.e("HomeViewModel", "Falha silenciosa ao sincronizar Token FCM", e)
                        }
                    }
                }
            }
        }
    }

    fun dismissAlert() {
        try {
            Log.d("HomeViewModel", "Alerta dispensado pelo usuario")
            _activeAlert.value = null
        } catch (e: Exception) {
            Log.e("HomeViewModel", "Erro ao dispensar alerta", e)
        }
    }
}