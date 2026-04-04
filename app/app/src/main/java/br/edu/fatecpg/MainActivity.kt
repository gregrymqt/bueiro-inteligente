package br.edu.fatecpg

import android.os.Bundle
import android.util.Log
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import br.edu.fatecpg.core.di.AppContainer

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

            Log.d("MainActivity", "Criando Activity Principal. Inicializando container de Injecao.")
            // Para usar o Rendere a rede local de forma dinâmica, configure sua BASE_URL no build.gradle.kts
            // Exemplo: buildConfigField("String", "BASE_URL", "\"https://bueiro-inteligente-back.onrender.com/\"")
            val baseUrl = br.edu.fatecpg.BuildConfig.BASE_URL.takeIf { it.isNotEmpty() } ?: "http://10.0.2.2:8000/"

            val appContainer = AppContainer(this, baseUrl)

            setContent {
                AppNavigation(appContainer = appContainer)
            }
    }
}
