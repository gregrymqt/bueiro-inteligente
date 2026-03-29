package br.edu.fatecpg

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.core.network.TokenManager

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val tokenManager = TokenManager(this)
        
        // Substitua pelo IP da sua máquina na rede local para testar no celular físico
        // Exemplo: "http://192.168.1.15:8000/"
        val baseUrl = "http://192.168.x.x:8000/"
        
        ApiClient.init(tokenManager, baseUrl)

        setContent {
            AppNavigation(tokenManager = tokenManager, baseUrl = baseUrl)
        }
    }
}