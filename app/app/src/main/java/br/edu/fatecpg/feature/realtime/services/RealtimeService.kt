package br.edu.fatecpg.feature.realtime.services

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.client.RealtimeWebSocketClient
import kotlinx.coroutines.flow.SharedFlow

class RealtimeService(
    private val client: RealtimeWebSocketClient
) {
    val alertas: SharedFlow<DrainStatusDTO> = client.drainStatusFlow
    val connectionError: SharedFlow<String?> = client.connectionErrorFlow       

    fun connect(token: String?) {
        try {
            Log.d("RealtimeService", "Solicitando conexao de WebSocket ao Client com Token ${if(token != null) "Presente" else "Nulo"}")
            client.connect(token)
        } catch (e: Exception) {
            Log.e("RealtimeService", "Falha critica ao repassar comando open de conexao websocket", e)
        }
    }

    fun disconnect() {
        try {
            Log.d("RealtimeService", "Desligamento voluntario do websocket solicitado")
            client.disconnect()
        } catch (e: Exception) {
            Log.e("RealtimeService", "Falha severa ao repassar comando de fechar socket via RealtimeService", e)
        }
    }
}
