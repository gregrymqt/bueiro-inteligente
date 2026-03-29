package br.edu.fatecpg.feature.realtime.services

import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.client.RealtimeWebSocketClient
import kotlinx.coroutines.flow.SharedFlow

class RealtimeService(
    private val client: RealtimeWebSocketClient
) {
    val alertas: SharedFlow<DrainStatusDTO> = client.drainStatusFlow
    val connectionError: SharedFlow<String?> = client.connectionErrorFlow

    fun connect(token: String?) {
        client.connect(token)
    }

    fun disconnect() {
        client.disconnect()
    }
}
