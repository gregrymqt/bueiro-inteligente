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

        Log.d("MainActivity", "Criando Activity Principal.")

        appContainer = (application as BueiroApplication).appContainer

        setContent {
            AppNavigation(appContainer = appContainer)
        }
    }

    override fun onDestroy() {
        super.onDestroy()
    }
}