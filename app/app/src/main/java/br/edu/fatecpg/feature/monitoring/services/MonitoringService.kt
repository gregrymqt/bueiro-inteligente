package br.edu.fatecpg.feature.monitoring.services

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

    companion object {
        fun create(): MonitoringService {
            return ApiClient.createService(MonitoringService::class.java)
        }
    }
}
