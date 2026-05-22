package br.edu.fatecpg.feature.device.services

import br.edu.fatecpg.feature.device.dto.DeviceTokenRequest
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface DeviceService {
    @POST("devices/token")
    suspend fun registerToken(@Body body: DeviceTokenRequest): Response<Unit>
}
