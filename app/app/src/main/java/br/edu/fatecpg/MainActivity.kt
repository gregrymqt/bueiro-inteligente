package br.edu.fatecpg

import android.os.Bundle
import android.view.View
import androidx.appcompat.app.AppCompatActivity
import androidx.navigation.fragment.NavHostFragment
import androidx.navigation.ui.setupWithNavController
import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.core.network.TokenManager
import com.google.android.material.bottomnavigation.BottomNavigationView

class MainActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // 1. Inicializa o cliente na raíz
        val tokenManager = TokenManager(this)
        ApiClient.init(tokenManager)

        val navHostFragment = supportFragmentManager
            .findFragmentById(R.id.nav_host_fragment) as NavHostFragment
        val navController = navHostFragment.navController
        val bottomNav = findViewById<BottomNavigationView>(R.id.bottom_nav)

        // 2. Conecta Bottom Navigation aos fragmentos do jetpack Navigation
        bottomNav.setupWithNavController(navController)

        // 3. Regra de UX: Esconder a barra na tela de Login
        navController.addOnDestinationChangedListener { _, destination, _ ->
            if (destination.id == R.id.nav_login) {
                bottomNav.visibility = View.GONE
            } else {
                bottomNav.visibility = View.VISIBLE
            }
        }

        // 4. Fluxo de Login: Vai para o login automaticamente se não há token no TokenManager
        if (tokenManager.getToken().isNullOrEmpty()) {
            navController.navigate(R.id.nav_login)
        }
    }
}