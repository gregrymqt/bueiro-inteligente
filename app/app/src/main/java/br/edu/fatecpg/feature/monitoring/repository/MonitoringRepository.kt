package br.edu.fatecpg.feature.monitoring.repository

import android.util.Log
import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class MonitoringRepository(
    private val monitoringService: MonitoringService,
    private val localCacheService: LocalCacheService
) {
    private companion object {
        private const val CACHE_TTL_MILLIS = 60 * 60 * 1000L
        private const val ALL_DRAINS_CACHE_KEY = "monitoring:drains:all"

        fun drainStatusCacheKey(bueiroId: String): String = "monitoring:drains:$bueiroId"
    }

    suspend fun getAllDrains(): Result<List<DrainStatusDTO>> {
        return withContext(Dispatchers.IO) {
            val responseResult = monitoringService.getAllDrains()
            
            if (responseResult.isSuccess) {
                val drains = responseResult.getOrThrow()
                // Opcional: Atualiza o cache
                localCacheService.getOrSet(
                    key = ALL_DRAINS_CACHE_KEY,
                    expiryMillis = CACHE_TTL_MILLIS
                ) { drains.toTypedArray() }
                
                Result.success(drains)
            } else {
                val cached = localCacheService.get<Array<DrainStatusDTO>>(ALL_DRAINS_CACHE_KEY)
                if (cached != null) {
                    Result.success(cached.toList())
                } else {
                    Result.failure(responseResult.exceptionOrNull() ?: Exception("Unknown error"))
                }
            }
        }
    }

    suspend fun getDrainStatus(bueiroId: String): Result<DrainStatusDTO> {
        return withContext(Dispatchers.IO) {
            val responseResult = monitoringService.getDrainStatus(bueiroId)
            
            if (responseResult.isSuccess) {
                val drain = responseResult.getOrThrow()
                localCacheService.getOrSet(
                    key = drainStatusCacheKey(bueiroId),
                    expiryMillis = CACHE_TTL_MILLIS
                ) { drain }
                Result.success(drain)
            } else {
                Result.failure(responseResult.exceptionOrNull() ?: Exception("Unknown error"))
            }
        }
    }
}
