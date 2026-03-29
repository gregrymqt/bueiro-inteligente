package br.edu.fatecpg.feature.home.viewmodel

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class HomeViewModel(
    private val realtimeRepository: RealtimeRepository,
    private val tokenManager: TokenManager
) : ViewModel() {

    private val _activeAlert = MutableStateFlow<DrainStatusDTO?>(null)
    val activeAlert: StateFlow<DrainStatusDTO?> = _activeAlert.asStateFlow()

    private val _connectionError = MutableStateFlow<String?>(null)
    val connectionError: StateFlow<String?> = _connectionError.asStateFlow()

    init {
        try {
            Log.d("HomeViewModel", "Inicializando HomeViewModel. Tentando conectar a websocket.")
            realtimeRepository.connect(tokenManager.getToken())
        } catch (e: Exception) {
            Log.e("HomeViewModel", "Erro inicial ao conectar WebSocket", e)
        }

        viewModelScope.launch(Dispatchers.Main) {
            try {
                realtimeRepository.alertas.collect { status ->
                    try {
                        val currentStatus = status.status.lowercase()
                        if (currentStatus == "alerta" || currentStatus == "crítico" || currentStatus == "critico") {
                            Log.i("HomeViewModel", "Alerta recebido para o bueiro: ${status.idBueiro}")
                            _activeAlert.value = status
                        }
                    } catch (e: Exception) {
                        Log.w("HomeViewModel", "Erro ao checar status do alerta", e)
                    }
                }
            } catch (e: Exception) {
                Log.e("HomeViewModel", "Erro critico ao processar fluxo de alertas", e)
            }
        }

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

    fun dismissAlert() {
        try {
            Log.d("HomeViewModel", "Alerta dispensado pelo usuario")
            _activeAlert.value = null
        } catch (e: Exception) {
            Log.e("HomeViewModel", "Erro ao dispensar alerta", e)
        }
    }
}
