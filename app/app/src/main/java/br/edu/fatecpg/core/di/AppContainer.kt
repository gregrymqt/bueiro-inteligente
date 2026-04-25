package br.edu.fatecpg.core.di

import android.content.Context
import android.util.Log
import androidx.room.Room
import br.edu.fatecpg.core.data.local.AppDatabase
import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.core.navigation.AndroidLocationHandler
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import br.edu.fatecpg.feature.auth.services.AuthService
import br.edu.fatecpg.feature.auth.viewmodel.LoginViewModelFactory
import br.edu.fatecpg.feature.auth.viewmodel.RegisterViewModelFactory
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModelFactory
import br.edu.fatecpg.feature.profile.repository.ProfileRepository
import br.edu.fatecpg.feature.profile.services.ProfileService
import br.edu.fatecpg.feature.profile.viewmodel.ProfileViewModelFactory
import br.edu.fatecpg.feature.realtime.client.RealtimeWebSocketClient
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import br.edu.fatecpg.feature.realtime.services.RealtimeService
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModelFactory
import com.google.gson.Gson

/**
 * Container de Injeo de Dependncias manual (Service Locator).
 * Mantm instncias globais nicas (Singleton/Lazy) para o ciclo de vida do aplicativo.
 */
class AppContainer(private val context: Context, private val baseUrl: String, private val wsUrl: String) {
    private val appContext = context.applicationContext
    private val gson: Gson by lazy { Gson() }
    private var roomDatabase: AppDatabase? = null

    val tokenManager: TokenManager by lazy {
        try {
            Log.i("AppContainer", "Criando TokenManager")
            TokenManager(appContext)
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

    private val localCacheService: LocalCacheService by lazy {
        LocalCacheService(database.cacheDao(), gson)
    }

    // --- Monitoring Feature ---
    private val monitoringService: MonitoringService by lazy { ApiClient.createService(MonitoringService::class.java) }
    private val monitoringRepository: MonitoringRepository by lazy {
        MonitoringRepository(monitoringService, localCacheService)
    }

    // --- Profile Feature ---
    private val profileService: ProfileService by lazy { ApiClient.createService(ProfileService::class.java) }
    private val profileRepository: ProfileRepository by lazy { ProfileRepository(profileService) }

    // --- Realtime/Home Feature ---
    private val realtimeWebSocketClient: RealtimeWebSocketClient by lazy {
        try {
            Log.i("AppContainer", "Criando RealtimeWebSocketClient para url: $wsUrl usando OkHttpClient compartilhado")
            RealtimeWebSocketClient(
                okHttpClient = ApiClient.httpClient,
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

    val authViewModelFactory by lazy { LoginViewModelFactory(authRepository) }

    val registerViewModelFactory by lazy { RegisterViewModelFactory(authRepository) }

    val homeViewModelFactory by lazy { HomeViewModelFactory(realtimeRepository, tokenManager) }

    val monitoringViewModelFactory by lazy { MonitoringViewModelFactory(monitoringRepository, locationHandler) }

    val profileViewModelFactory by lazy { ProfileViewModelFactory(profileRepository) }

    fun close() {
        try {
            Log.i("AppContainer", "Fechando recursos do AppContainer")
            roomDatabase?.close()
            roomDatabase = null
        } catch (e: Exception) {
            Log.e("AppContainer", "Erro ao fechar o banco local Room", e)
        }
    }

    private val database: AppDatabase
        get() {
            roomDatabase?.let { return it }

            return try {
                Log.i("AppContainer", "Inicializando Room Database local")
                Room.databaseBuilder(
                    appContext,
                    AppDatabase::class.java,
                    "bueiro_inteligente_cache.db"
                )
                    .fallbackToDestructiveMigration()
                    .build()
                    .also { roomDatabase = it }
            } catch (e: Exception) {
                Log.e("AppContainer", "Erro ao criar o Room Database local", e)
                throw e
            }
        }
}
