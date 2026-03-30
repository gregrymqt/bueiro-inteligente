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
            // Substitua pelo IP da sua maquina na rede local para testar no celular fisico
            // Exemplo: "http://192.168.1.15:8000/"
            val baseUrl = "http://192.168.x.x:8000/"

            val appContainer = AppContainer(this, baseUrl)

            setContent {
                AppNavigation(appContainer = appContainer)
            }
    }
}
