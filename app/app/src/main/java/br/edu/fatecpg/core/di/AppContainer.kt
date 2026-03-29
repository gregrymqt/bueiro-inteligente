package br.edu.fatecpg.core.di

import android.content.Context
import android.util.Log
import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import br.edu.fatecpg.feature.auth.services.AuthService
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository        
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import br.edu.fatecpg.feature.profile.repository.ProfileRepository
import br.edu.fatecpg.feature.profile.services.ProfileService
import br.edu.fatecpg.feature.realtime.client.RealtimeWebSocketClient
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import br.edu.fatecpg.feature.realtime.services.RealtimeService
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.core.navigation.AndroidLocationHandler

/**
 * Container de Injeção de Dependências manual (Service Locator).
 * Mantém instâncias globais únicas (Singleton/Lazy) para o ciclo de vida do aplicativo.
 */
class AppContainer(private val context: Context, private val baseUrl: String) { 

    val tokenManager: TokenManager by lazy {
        try {
            Log.i("AppContainer", "Criando TokenManager")
            TokenManager(context)
        } catch (e: Exception) {
            Log.e("AppContainer", "Erro ao criar TokenManager", e)
            throw e
        }
    }

    val locationHandler: LocationHandler by lazy {
        try {
            Log.i("AppContainer", "Criando LocationHandler")
            AndroidLocationHandler(context)
        } catch (e: Exception) {
            Log.e("AppContainer", "Erro ao criar LocationHandler", e)
            throw e
        }
    }

    init {
        try {
            Log.i("AppContainer", "Inicializando AppContainer")
            // Inicializa o ApiClient para todo o aplicativo
            ApiClient.init(tokenManager, baseUrl)
            Log.i("AppContainer", "ApiClient inicializado via AppContainer")
        } catch (e: Exception) {
            Log.e("AppContainer", "Falha critica no INIT do AppContainer", e)
        }
    }

    // --- Auth Feature ---
    val authService: AuthService by lazy { ApiClient.createService(AuthService::class.java) }
    val authRepository: AuthRepository by lazy { AuthRepository(authService, tokenManager) }

    // --- Monitoring Feature ---
    val monitoringService: MonitoringService by lazy { ApiClient.createService(MonitoringService::class.java) }
    val monitoringRepository: MonitoringRepository by lazy { MonitoringRepository(monitoringService) }

    // --- Profile Feature ---
    val profileService: ProfileService by lazy { ApiClient.createService(ProfileService::class.java) }
    val profileRepository: ProfileRepository by lazy { ProfileRepository(profileService) }

    // --- Realtime/Home Feature ---
    val realtimeWebSocketClient: RealtimeWebSocketClient by lazy {
        try {
            val wsUrl = baseUrl.replace("http://", "ws://").replace("https://", "wss://") + "realtime/ws"
            Log.i("AppContainer", "Criando RealtimeWebSocketClient para url: $wsUrl")
            RealtimeWebSocketClient(
                okHttpClient = okhttp3.OkHttpClient(),
                gson = com.google.gson.Gson(),
                baseUrl = wsUrl
            )
        } catch (e: Exception) {
            Log.e("AppContainer", "Erro ao criar RealtimeWebSocketClient", e)
            throw e
        }
    }
    val realtimeService: RealtimeService by lazy { RealtimeService(realtimeWebSocketClient) }
    val realtimeRepository: RealtimeRepository by lazy { RealtimeRepository(realtimeService) }
}
