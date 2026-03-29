package br.edu.fatecpg.feature.monitoring.repository

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.IOException

class MonitoringRepository(private val monitoringService: MonitoringService) {  

    suspend fun getAllDrains(): Result<List<DrainStatusDTO>> {
        return withContext(Dispatchers.IO) {
            try {
                Log.d("MonitoringRepository", "Buscando todos os bueiros na API")
                val response = monitoringService.getAllDrains()
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null) {
                        Log.i("MonitoringRepository", "Bueiros resgatados com sucesso: ${body.size} encontrados")
                        Result.success(body)
                    } else {
                        Log.w("MonitoringRepository", "Acesso concluido mas com a lista de bueiros vazia (body null).")
                        Result.failure(Exception("A lista de bueiros retornou vazia."))
                    }
                } else {
                    Log.w("MonitoringRepository", "Erro na resposta da API ao listar bueiros. Codigo: ${response.code()}")
                    when (response.code()) {
                        403 -> Result.failure(Exception("Acesso negado para listar os bueiros."))
                        else -> Result.failure(Exception("Erro na requisição: Código HTTP ${response.code()}"))
                    }
                }
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
                Log.d("MonitoringRepository", "Buscando status do bueiro ID: $bueiroId")
                val response = monitoringService.getDrainStatus(bueiroId)       
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null) {
                        Log.i("MonitoringRepository", "Status do bueiro $bueiroId resgatado.")
                        Result.success(body)
                    } else {
                        Log.w("MonitoringRepository", "Resposta vazia ao tentar pegar o bueiro $bueiroId")
                        Result.failure(Exception("Resposta vazia da API."))     
                    }
                } else {
                    Log.w("MonitoringRepository", "Falha na resposta HTTP (getDrainStatus): ${response.code()}")
                    Result.failure(Exception("Erro na requisição: Código HTTP ${response.code()}"))
                }
            } catch (e: IOException) {
                Log.e("MonitoringRepository", "Erro de rede no bueiro $bueiroId", e)
                Result.failure(Exception("Falha de rede. Verifique sua conexão de internet.", e))
            } catch (e: Exception) {
                Log.e("MonitoringRepository", "Falha critica não de rede ao buscar bueiro ID $bueiroId", e)
                Result.failure(Exception("Ocorreu um erro inesperado: ${e.localizedMessage}", e))
            }
        }
    }
}
