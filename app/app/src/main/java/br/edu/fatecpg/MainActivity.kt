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
        ApiClient.init(tokenManager)

        setContent {
            AppNavigation(tokenManager = tokenManager)
        }
    }
}