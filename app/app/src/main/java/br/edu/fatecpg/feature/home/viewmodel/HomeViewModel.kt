package br.edu.fatecpg.feature.home.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.core.notifications.NotificationHelper
import br.edu.fatecpg.feature.device.repository.DeviceRepository
import br.edu.fatecpg.feature.home.dto.HomeResponseDTO
import br.edu.fatecpg.feature.home.dto.StatCardDTO
import br.edu.fatecpg.feature.home.repository.HomeRepository
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import com.google.firebase.messaging.FirebaseMessaging
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import kotlin.time.Duration.Companion.milliseconds

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

    // --- TRACKING PARA TEMPO REAL ---
    private val bueiroStatusMap = mutableMapOf<String, DrainStatusDTO>()

    init {
        try {
            Log.d("HomeViewModel", "Inicializando HomeViewModel. Conectando WebSocket e buscando dados.")
            realtimeRepository.connect(tokenManager.getToken())
            loadHomeContent() // <-- Dispara o carregamento dos Stat Cards e Carousels
            syncDeviceToken()
        } catch (e: Exception) {
            Log.e("HomeViewModel", "Erro inicial no HomeViewModel", e)
        }

        // Observa Alertas Críticos e Atualiza Estatísticas em Tempo Real
        viewModelScope.launch(Dispatchers.Main) {
            try {
                realtimeRepository.alertas
                    .onEach { status ->
                        // 1. Lógica de Alerta Imediato (Card de Alerta)
                        processIncomingAlert(status)
                    }
                    .sample(600.milliseconds) // Otimização: evita recomposições excessivas da malha de cards
                    .collect { status ->
                        // 2. Lógica de Recalculo das Estatísticas da Home
                        updateHomeStatsRealtime(status)
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

    private fun processIncomingAlert(status: DrainStatusDTO) {
        try {
            val currentStatus = status.status?.lowercase() ?: ""
            if (currentStatus == "alerta" || currentStatus == "crítico" || currentStatus == "critico") {
                Log.i("HomeViewModel", "Alerta em tempo real: ${status.name}")
                _activeAlert.value = status

                if (currentStatus == "crítico" || currentStatus == "critico") {
                    notificationHelper.showCriticalNotification(status)
                }
            }
        } catch (e: Exception) {
            Log.w("HomeViewModel", "Erro ao processar alerta", e)
        }
    }

    /**
     * Atualiza dinamicamente os StatCards do HomeUiState.Success baseando-se no novo status recebido.
     * Implementa o cálculo matemático exato de variação Delta global.
     */
    private fun updateHomeStatsRealtime(newStatus: DrainStatusDTO) {
        val currentState = _uiState.value
        if (currentState !is HomeUiState.Success) return

        val previousStatus = bueiroStatusMap[newStatus.hardwareId]
        bueiroStatusMap[newStatus.hardwareId] = newStatus

        val totalManholes = findTotalManholes(currentState.data.stats)

        val updatedStats = currentState.data.stats.map { stat ->
            when {
                isAlertStat(stat.title) -> updateAlertCount(stat, newStatus, previousStatus)
                isObstructionStat(stat.title) -> updateObstructionAverage(stat, newStatus, previousStatus, totalManholes)
                else -> stat
            }
        }

        _uiState.value = HomeUiState.Success(
            currentState.data.copy(stats = updatedStats)
        )
    }

    private fun findTotalManholes(stats: List<StatCardDTO>): Double {
        return stats.find {
            it.title.contains("Total", ignoreCase = true) ||
            it.title.contains("Sensores", ignoreCase = true) ||
            it.title.contains("Bueiros", ignoreCase = true)
        }?.value?.filter { it.isDigit() }?.toDoubleOrNull() ?: 10.0
    }

    private fun isAlertStat(title: String): Boolean {
        return title.contains("Alerta", ignoreCase = true) ||
                title.contains("Crítico", ignoreCase = true) ||
                title.contains("Critico", ignoreCase = true)
    }

    private fun isObstructionStat(title: String): Boolean {
        return title.contains("Obstrução", ignoreCase = true) ||
                title.contains("Média", ignoreCase = true) ||
                title.contains("Media", ignoreCase = true)
    }

    private fun updateAlertCount(stat: StatCardDTO, newStatus: DrainStatusDTO, previousStatus: DrainStatusDTO?): StatCardDTO {
        var count = stat.value.toIntOrNull() ?: 0
        val isNowCritical = newStatus.status?.lowercase() in listOf("alerta", "crítico", "critico")
        val wasCritical = previousStatus?.status?.lowercase() in listOf("alerta", "crítico", "critico")

        if (isNowCritical && !wasCritical) count++
        else if (!isNowCritical && wasCritical) count--

        return stat.copy(value = count.coerceAtLeast(0).toString())
    }

    private fun updateObstructionAverage(stat: StatCardDTO, newStatus: DrainStatusDTO, previousStatus: DrainStatusDTO?, totalManholes: Double): StatCardDTO {
        val newNivel = newStatus.nivelObstrucao ?: return stat
        val currentAvg = stat.value.replace("%", "").replace(",", ".").toDoubleOrNull() ?: 0.0
        val previousNivel = previousStatus?.nivelObstrucao ?: currentAvg

        val delta = newNivel - previousNivel
        val newAvg = currentAvg + (delta / totalManholes)

        return stat.copy(value = "${String.format("%.1f", newAvg)}%".replace(".", ","))
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