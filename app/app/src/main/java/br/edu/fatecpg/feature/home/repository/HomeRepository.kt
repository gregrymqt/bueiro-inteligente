package br.edu.fatecpg.feature.home.repository

import android.util.Log
import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.feature.home.dto.HomeResponseDTO
import br.edu.fatecpg.feature.home.services.HomeService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class HomeRepository(
    private val homeService: HomeService,
    private val localCacheService: LocalCacheService
) {
    companion object {
        private const val CACHE_KEY_HOME_CONTENT = "home_content_public"
        // Cache de 1 hora (em milissegundos) mantendo paridade com o backend
        private const val CACHE_TTL_MS = 3600_000L
    }

    /**
     * Obtém o conteúdo da Home, priorizando o cache local do Room.
     * Caso expire ou não exista, consome a API remota e atualiza o cache automaticamente.
     */
    suspend fun getHomeContent(): Result<HomeResponseDTO> {
        return withContext(Dispatchers.IO) {
            try {
                Log.d("HomeRepository", "Buscando conteúdo da Home (verificando LocalCacheService)")

                // Utiliza o getOrSet reificado que você construiu
                val data = localCacheService.getOrSet(
                    key = CACHE_KEY_HOME_CONTENT,
                    expiryMillis = CACHE_TTL_MS
                ) {
                    Log.i("HomeRepository", "Cache ausente/expirado. Consumindo API via HomeService.")
                    homeService.getHomeContent()
                }

                Result.success(data)
            } catch (e: Exception) {
                Log.e("HomeRepository", "Erro crítico ao buscar conteúdo da Home: ${e.message}", e)
                Result.failure(e)
            }
        }
    }
}