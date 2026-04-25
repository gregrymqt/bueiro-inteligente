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
        // Para usar o Render e a rede local de forma dinâmica, configure sua BASE_URL no build.gradle.kts.
        // Exemplo: buildConfigField("String", "BASE_URL", "\"https://bueiro-inteligente-back.onrender.com/\"")
        val baseUrl = br.edu.fatecpg.BuildConfig.BASE_URL.takeIf { it.isNotEmpty() }
            ?: "http://10.0.2.2:8000/"

        // Replicando o comportamento dinâmico para o WebSocket de tempo real (WSS em produção, WS em dev)
        val wsUrl = br.edu.fatecpg.BuildConfig.BASE_URL.takeIf { it.isNotEmpty() }
            ?.replace("https://", "wss://")
            ?.replace("http://", "ws://")
            ?.plus("realtime/ws")
            ?: "ws://10.0.2.2:8000/realtime/ws"

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
