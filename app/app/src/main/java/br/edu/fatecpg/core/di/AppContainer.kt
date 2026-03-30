package br.edu.fatecpg.core.di

import android.content.Context
import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.core.navigation.AndroidLocationHandler
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import br.edu.fatecpg.feature.auth.services.AuthService
import br.edu.fatecpg.feature.auth.viewmodel.LoginViewModel
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel
import br.edu.fatecpg.feature.profile.repository.ProfileRepository
import br.edu.fatecpg.feature.profile.services.ProfileService
import br.edu.fatecpg.feature.profile.viewmodel.ProfileViewModel
import br.edu.fatecpg.feature.realtime.client.RealtimeWebSocketClient
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import br.edu.fatecpg.feature.realtime.services.RealtimeService

/**
 * Container de Injeo de Dependncias manual (Service Locator).
 * Mantm instncias globais nicas (Singleton/Lazy) para o ciclo de vida do aplicativo.
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
    private val authService: AuthService by lazy { ApiClient.createService(AuthService::class.java) }
    private val authRepository: AuthRepository by lazy { AuthRepository(authService, tokenManager) }

    // --- Monitoring Feature ---
    private val monitoringService: MonitoringService by lazy { ApiClient.createService(MonitoringService::class.java) }
    private val monitoringRepository: MonitoringRepository by lazy { MonitoringRepository(monitoringService) }

    // --- Profile Feature ---
    private val profileService: ProfileService by lazy { ApiClient.createService(ProfileService::class.java) }
    private val profileRepository: ProfileRepository by lazy { ProfileRepository(profileService) }

    // --- Realtime/Home Feature ---
    private val realtimeWebSocketClient: RealtimeWebSocketClient by lazy {
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
    private val realtimeService: RealtimeService by lazy { RealtimeService(realtimeWebSocketClient) }
    private val realtimeRepository: RealtimeRepository by lazy { RealtimeRepository(realtimeService) }


    // --- ViewModel Factories ---

    val authViewModelFactory: ViewModelProvider.Factory = object : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            if (modelClass.isAssignableFrom(LoginViewModel::class.java)) {
                @Suppress("UNCHECKED_CAST")
                return LoginViewModel(authRepository) as T
            }
            throw IllegalArgumentException("Unknown ViewModel class")
        }
    }

    val homeViewModelFactory: ViewModelProvider.Factory = object : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            if (modelClass.isAssignableFrom(HomeViewModel::class.java)) {
                @Suppress("UNCHECKED_CAST")
                return HomeViewModel(realtimeRepository, tokenManager) as T
            }
            throw IllegalArgumentException("Unknown ViewModel class")
        }
    }

    val monitoringViewModelFactory: ViewModelProvider.Factory = object : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            if (modelClass.isAssignableFrom(MonitoringViewModel::class.java)) {
                @Suppress("UNCHECKED_CAST")
                return MonitoringViewModel(monitoringRepository, locationHandler) as T
            }
            throw IllegalArgumentException("Unknown ViewModel class")
        }
    }

    val profileViewModelFactory: ViewModelProvider.Factory = object : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            if (modelClass.isAssignableFrom(ProfileViewModel::class.java)) {
                @Suppress("UNCHECKED_CAST")
                return ProfileViewModel(profileRepository) as T
            }
            throw IllegalArgumentException("Unknown ViewModel class")
        }
    }
}
