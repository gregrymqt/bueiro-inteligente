package br.edu.fatecpg

import android.os.Bundle
import android.util.Log
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import br.edu.fatecpg.core.di.AppContainer

class MainActivity : ComponentActivity() {
    private lateinit var appContainer: AppContainer

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        Log.d("MainActivity", "Criando Activity Principal. Inicializando container de Injecao.")

        // 1. Limpa espaços e garante que a URL base termine com "/" para evitar erros de concatenação
        val rawBaseUrl = BuildConfig.BASE_URL.trim()
        val cleanRootUrl = if (rawBaseUrl.isNotEmpty() && !rawBaseUrl.endsWith("/")) "$rawBaseUrl/" else rawBaseUrl

        // 2. Seta o valor do api/v1/ exclusivamente para as chamadas HTTP das features/services
        val baseUrl = cleanRootUrl.takeIf { it.isNotEmpty() }
            ?.plus("api/v1/")
            ?: "http://10.0.2.2:8080/api/v1/"

        // 3. Monta o WebSocket direto na raiz, impedindo que o "api/v1/" contamine o SignalR Hub
        val wsUrl = cleanRootUrl.takeIf { it.isNotEmpty() }
            ?.replace("https://", "wss://")
            ?.replace("http://", "ws://")
            ?.plus("realtime/ws")
            ?: "ws://10.0.2.2:8080/realtime/ws"

        Log.i("MainActivity", "🌐 HTTP REST configurado para: $baseUrl")
        Log.i("MainActivity", "🔌 WebSocket SignalR configurado para: $wsUrl")

        appContainer = AppContainer(this, baseUrl, wsUrl)

        setContent {
            AppNavigation(appContainer = appContainer)
        }
    }

    override fun onDestroy() {
        try {
            if (::appContainer.isInitialized) {
                appContainer.close()
            }
        } catch (e: Exception) {
            Log.e("MainActivity", "Erro ao encerrar o AppContainer", e)
        }

        super.onDestroy()
    }
}