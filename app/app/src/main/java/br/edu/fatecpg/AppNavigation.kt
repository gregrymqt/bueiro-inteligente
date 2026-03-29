package br.edu.fatecpg

import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.List
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import br.edu.fatecpg.feature.auth.services.AuthService
import br.edu.fatecpg.feature.auth.ui.LoginScreen
import br.edu.fatecpg.feature.auth.viewmodel.LoginViewModel
import br.edu.fatecpg.feature.auth.viewmodel.LoginViewModelFactory
import br.edu.fatecpg.feature.home.ui.HomeScreen
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModelFactory
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import br.edu.fatecpg.feature.realtime.services.RealtimeService
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import br.edu.fatecpg.feature.monitoring.ui.MonitoringScreen
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModelFactory

sealed class Screen(val route: String, val title: String, val icon: androidx.compose.ui.graphics.vector.ImageVector?) {
    object Login : Screen("login", "Login", null)
    object Home : Screen("home", "Home", Icons.Default.Home)
    object Monitoring : Screen("monitoring", "Bueiros", Icons.Default.List)
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppNavigation(tokenManager: TokenManager, baseUrl: String) {
    val navController = rememberNavController()
    
    // Rota inicial dependendo de ter token salvo ou não
    val startDestination = if (tokenManager.getToken().isNullOrEmpty()) {
        Screen.Login.route
    } else {
        Screen.Home.route
    }

    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentDestination = navBackStackEntry?.destination

    // Oculta bottom bar na tela de login
    val showBottomBar = currentDestination?.route != Screen.Login.route

    Scaffold(
        topBar = {
            if (showBottomBar) {
                TopAppBar(
                    title = { Text(text = "Bueiro Inteligente") },
                    actions = {
                        IconButton(onClick = {
                            tokenManager.clearToken()
                            navController.navigate(Screen.Login.route) {
                                popUpTo(0) { inclusive = true } // Limpa a pilha de navegação
                            }
                        }) {
                            Icon(imageVector = Icons.Default.ExitToApp, contentDescription = "Sair")
                        }
                    },
                    colors = TopAppBarDefaults.topAppBarColors(
                        containerColor = MaterialTheme.colorScheme.primaryContainer,
                        titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                    )
                )
            }
        },
        bottomBar = {
            if (showBottomBar) {
                NavigationBar {
                    val items = listOf(Screen.Home, Screen.Monitoring)
                    items.forEach { screen ->
                        NavigationBarItem(
                            icon = { screen.icon?.let { Icon(it, contentDescription = screen.title) } },
                            label = { Text(screen.title) },
                            selected = currentDestination?.hierarchy?.any { it.route == screen.route } == true,
                            onClick = {
                                navController.navigate(screen.route) {
                                    popUpTo(navController.graph.findStartDestination().id) {
                                        saveState = true
                                    }
                                    launchSingleTop = true
                                    restoreState = true
                                }
                            }
                        )
                    }
                }
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = startDestination,
            modifier = Modifier.padding(innerPadding)
        ) {
            composable(Screen.Login.route) {
                val authService = AuthService.create()
                val repository = AuthRepository(authService, tokenManager)
                val viewModel: LoginViewModel = viewModel(factory = LoginViewModelFactory(repository))
                
                LoginScreen(
                    viewModel = viewModel,
                    onNavigateToHome = {
                        navController.navigate(Screen.Home.route) {
                            popUpTo(Screen.Login.route) { inclusive = true }
                        }
                    }
                )
            }
            
            composable(Screen.Home.route) {
                val wsUrl = baseUrl.replace("http://", "ws://").replace("https://", "wss://") + "realtime/ws"
                
                val okHttpClient = okhttp3.OkHttpClient()
                val gson = com.google.gson.Gson()
                val websocketClient = br.edu.fatecpg.feature.realtime.client.RealtimeWebSocketClient(
                    okHttpClient = okHttpClient,
                    gson = gson,
                    baseUrl = wsUrl
                )
                val realtimeService = RealtimeService(websocketClient)
                val realtimeRepository = RealtimeRepository(realtimeService)    
                val viewModel: HomeViewModel = viewModel(factory = HomeViewModelFactory(realtimeRepository, tokenManager))
                HomeScreen(viewModel = viewModel)
            }
            
            composable(Screen.Monitoring.route) {
                val monitoringService = MonitoringService.create()
                val repository = MonitoringRepository(monitoringService)
                val viewModel: MonitoringViewModel = viewModel(factory = MonitoringViewModelFactory(repository))
                
                MonitoringScreen(viewModel = viewModel)
            }
        }
    }
}