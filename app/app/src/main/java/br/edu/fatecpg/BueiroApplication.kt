package br.edu.fatecpg

import android.app.Application
import android.util.Log
import br.edu.fatecpg.core.di.AppContainer

class BueiroApplication : Application() {
    lateinit var appContainer: AppContainer
        private set

    override fun onCreate() {
        super.onCreate()
        Log.d("BueiroApplication", "Inicializando Application")
        
        val rawBaseUrl = BuildConfig.BASE_URL.trim()
        val cleanRootUrl = if (rawBaseUrl.isNotEmpty() && !rawBaseUrl.endsWith("/")) "$rawBaseUrl/" else rawBaseUrl

        val baseUrl = cleanRootUrl.takeIf { it.isNotEmpty() }
            ?.plus("api/v1/")
            ?: "http://10.0.2.2:8080/api/v1/"

        val wsUrl = cleanRootUrl.takeIf { it.isNotEmpty() }
            ?.replace("https://", "wss://")
            ?.replace("http://", "ws://")
            ?.plus("realtime/ws")
            ?: "ws://10.0.2.2:8080/realtime/ws"

        appContainer = AppContainer(this, baseUrl, wsUrl)
    }
}
