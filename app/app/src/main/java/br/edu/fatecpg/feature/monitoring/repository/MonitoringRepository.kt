package br.edu.fatecpg.feature.monitoring.repository

import android.util.Log
import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.IOException

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
            try {
                Log.d("MonitoringRepository", "Buscando todos os bueiros com cache local")
                val cachedDrains = localCacheService.getOrSet(
                    key = ALL_DRAINS_CACHE_KEY,
                    expiryMillis = CACHE_TTL_MILLIS
                ) {
                    fetchAllDrainsFromNetwork()
                }

                Log.i(
                    "MonitoringRepository",
                    "Bueiros resgatados com sucesso: ${cachedDrains.size} encontrados"
                )
                Result.success(cachedDrains.toList())
            } catch (e: IOException) {
                Log.e("MonitoringRepository", "Falha de rede ao tentar listar os bueiros", e)
                Result.failure(Exception("Falha de rede. A API pode estar fora do ar ou sem internet.", e))
            } catch (e: Exception) {
                Log.e("MonitoringRepository", "Erro inesperado ao listar os bueiros", e)
                Result.failure(Exception("Ocorreu um erro inesperado: ${e.localizedMessage}", e))
            }
        }
    }

    suspend fun getDrainStatus(bueiroId: String): Result<DrainStatusDTO> {
        return withContext(Dispatchers.IO) {
            try {
                Log.d("MonitoringRepository", "Buscando status do bueiro ID: $bueiroId com cache local")
                val drainStatus = localCacheService.getOrSet(
                    key = drainStatusCacheKey(bueiroId),
                    expiryMillis = CACHE_TTL_MILLIS
                ) {
                    fetchDrainStatusFromNetwork(bueiroId)
                }

                Log.i("MonitoringRepository", "Status do bueiro $bueiroId resgatado.")
                Result.success(drainStatus)
            } catch (e: IOException) {
                Log.e("MonitoringRepository", "Erro de rede no bueiro $bueiroId", e)
                Result.failure(Exception("Falha de rede. Verifique sua conex�o de internet.", e))
            } catch (e: Exception) {
                Log.e("MonitoringRepository", "Falha critica n�o de rede ao buscar bueiro ID $bueiroId", e)
                Result.failure(Exception("Ocorreu um erro inesperado: ${e.localizedMessage}", e))
            }
        }
    }

    private suspend fun fetchAllDrainsFromNetwork(): Array<DrainStatusDTO> {
        val response = monitoringService.getAllDrains()

        if (!response.isSuccessful) {
            throw IllegalStateException(
                "Erro na requisição ao listar bueiros: código HTTP ${response.code()}"
            )
        }

        val body = response.body()
            ?: throw IllegalStateException("A lista de bueiros retornou vazia.")

        return body.toTypedArray()
    }

    private suspend fun fetchDrainStatusFromNetwork(bueiroId: String): DrainStatusDTO {
        val response = monitoringService.getDrainStatus(bueiroId)

        if (!response.isSuccessful) {
            throw IllegalStateException(
                "Erro na requisição ao buscar o bueiro $bueiroId: código HTTP ${response.code()}"
            )
        }

        return response.body()
            ?: throw IllegalStateException("Resposta vazia da API para o bueiro $bueiroId.")
    }
}
