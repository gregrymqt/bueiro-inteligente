package br.edu.fatecpg.feature.realtime.repository

import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.services.RealtimeService
import kotlinx.coroutines.flow.SharedFlow

class RealtimeRepository(private val realtimeService: RealtimeService) {

    val alertas: SharedFlow<DrainStatusDTO> = realtimeService.alertas
    val connectionError: SharedFlow<String?> = realtimeService.connectionError

    fun connect(token: String?) {
        realtimeService.connect(token)
    }

    fun disconnect() {
        realtimeService.disconnect()
    }
}
