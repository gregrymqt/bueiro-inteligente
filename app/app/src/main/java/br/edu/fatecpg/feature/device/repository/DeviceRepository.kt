package br.edu.fatecpg.feature.device.repository

import br.edu.fatecpg.feature.device.dto.DeviceTokenRequest
import br.edu.fatecpg.feature.device.services.DeviceService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class DeviceRepository(
    private val deviceService: DeviceService
) {
    suspend fun registerToken(fcmToken: String): Result<Unit> {
        return withContext(Dispatchers.IO) {
            try {
                val response = deviceService.registerToken(DeviceTokenRequest(fcmToken))
                if (response.isSuccessful) {
                    Result.success(Unit)
                } else {
                    Result.failure(Exception("Erro ao registrar token: ${response.code()}"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }
    }
}
