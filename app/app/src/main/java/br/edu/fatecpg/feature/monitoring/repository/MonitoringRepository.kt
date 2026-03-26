package br.edu.fatecpg.feature.monitoring.repository

import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.IOException

class MonitoringRepository(private val monitoringService: MonitoringService) {

    suspend fun getAllDrains(): Result<List<DrainStatusDTO>> {
        return withContext(Dispatchers.IO) {
            try {
                val response = monitoringService.getAllDrains()
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null) {
                        Result.success(body)
                    } else {
                        Result.failure(Exception("A lista de bueiros retornou vazia."))
                    }
                } else {
                    when (response.code()) {
                        401 -> Result.failure(Exception("Sessão expirada ou token inválido. Faça login novamente."))
                        403 -> Result.failure(Exception("Acesso negado para listar os bueiros."))
                        else -> Result.failure(Exception("Erro na requisição: Código HTTP ${response.code()}"))
                    }
                }
            } catch (e: IOException) {
                Result.failure(Exception("Falha de rede. A API pode estar fora do ar ou sem internet.", e))
            } catch (e: Exception) {
                Result.failure(Exception("Ocorreu um erro inesperado: ${e.localizedMessage}", e))
            }
        }
    }

    suspend fun getDrainStatus(bueiroId: String): Result<DrainStatusDTO> {
        return withContext(Dispatchers.IO) {
            try {
                val response = monitoringService.getDrainStatus(bueiroId)
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null) {
                        Result.success(body)
                    } else {
                        Result.failure(Exception("Resposta vazia da API."))
                    }
                } else {
                    Result.failure(Exception("Erro na requisição: Código HTTP ${response.code()}"))
                }
            } catch (e: IOException) {
                Result.failure(Exception("Falha de rede. Verifique sua conexão de internet.", e))
            } catch (e: Exception) {
                Result.failure(Exception("Ocorreu um erro inesperado: ${e.localizedMessage}", e))
            }
        }
    }
}
