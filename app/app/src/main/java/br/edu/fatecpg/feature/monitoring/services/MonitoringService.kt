package br.edu.fatecpg.feature.monitoring.services

import android.util.Log
import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path

interface MonitoringService {

    @GET("monitoring/{bueiro_id}/status")
    suspend fun getDrainStatus(
        @Path("bueiro_id") id: String
    ): Response<DrainStatusDTO>

    @GET("monitoring/all")
    suspend fun getAllDrains(): Response<List<DrainStatusDTO>>

    companion object {
        fun create(): MonitoringService {
            return try {
                Log.d("MonitoringService", "Criando instância do MonitoringService via ApiClient")
                ApiClient.createService(MonitoringService::class.java)
            } catch (e: Exception) {
                Log.e("MonitoringService", "Erro ao criar instância de MonitoringService", e)
                throw e
            }
        }
    }
}
