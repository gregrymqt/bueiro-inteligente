package br.edu.fatecpg.feature.realtime.repository

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.services.RealtimeService
import kotlinx.coroutines.flow.SharedFlow

class RealtimeRepository(private val realtimeService: RealtimeService) {        

    val alertas: SharedFlow<DrainStatusDTO> = realtimeService.alertas
    val connectionError: SharedFlow<String?> = realtimeService.connectionError  

    fun connect(token: String?) {
        try {
            Log.d("RealtimeRepository", "Sinalizando subida de websocket atraves de repositorio")
            realtimeService.connect(token)
        } catch (e: Exception) {
            Log.e("RealtimeRepository", "Erro crítico ao encaminhar start do servico de realtime", e)
        }
    }

    fun disconnect() {
        try {
            Log.d("RealtimeRepository", "Repassando queda voluntaria de socket client por parte da view")
            realtimeService.disconnect()
        } catch (e: Exception) {
            Log.e("RealtimeRepository", "Problema crasso ao despachar drop the conexao RT do ws", e)
        }
    }
}
