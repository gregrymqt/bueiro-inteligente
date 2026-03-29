package br.edu.fatecpg

import android.util.Log
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.*
import br.edu.fatecpg.core.di.AppContainer
import br.edu.fatecpg.feature.auth.ui.LoginScreen
import br.edu.fatecpg.feature.auth.viewmodel.AuthViewModel
import br.edu.fatecpg.feature.home.ui.HomeScreen
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel
import br.edu.fatecpg.feature.monitoring.ui.MonitoringScreen
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel
import br.edu.fatecpg.feature.profile.ui.ProfileScreen
import br.edu.fatecpg.feature.profile.viewmodel.ProfileViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppNavigation(appContainer: AppContainer) {
    try {
        val navController = rememberNavController()

        Scaffold(
            topBar = {
                val currentRoute = navController.currentBackStackEntryAsState().value?.destination?.route
                Log.d("AppNavigation", "Redesenhando TopBar. Rota atual: $currentRoute")
                
                if (currentRoute != "login") {
                    TopAppBar(
                        title = { Text(text = "Bueiro Inteligente", fontWeight = FontWeight.Bold) },
                        actions = {
                            IconButton(onClick = {
                                try {
                                    Log.i("AppNavigation", "Usuario clicou em Logout. Executando...")
                                    appContainer.tokenManager.clearToken()
                                    navController.navigate("login") {
                                        popUpTo(0) { inclusive = true }
                                    }
                                } catch (e: Exception) {
                                    Log.e("AppNavigation", "Erro ao executar logout", e)
                                }
                            }) {
                                Icon(Icons.Default.ExitToApp, contentDescription = "Sair")
                            }
                        },
                        colors = TopAppBarDefaults.topAppBarColors(
                            containerColor = MaterialTheme.colorScheme.primary,
                            titleContentColor = MaterialTheme.colorScheme.onPrimary,
                            actionIconContentColor = MaterialTheme.colorScheme.onPrimary
                        )
                    )
                }
            }
        ) { paddingValues ->
            NavHost(
                navController = navController,
                startDestination = "login",
                modifier = Modifier.padding(paddingValues)
            ) {
                composable("login") {
                    try {
                        Log.d("AppNavigation", "NavHost -> Criando LoginScreen")
                        val authViewModel: AuthViewModel = viewModel(factory = appContainer.authViewModelFactory)
                        
                        // Direciona para Home se ja tiver token salvo
                        LaunchedEffect(Unit) {
                            try {
                                if (appContainer.tokenManager.getToken() != null) {
                                    Log.i("AppNavigation", "Token encontrado. Redirecionando automaticamente para home.")
                                    navController.navigate("home") {
                                        popUpTo("login") { inclusive = true }
                                    }
                                }
                            } catch (e: Exception) {
                                Log.e("AppNavigation", "Erro ao checar token guardado na LoginScreen", e)
                            }
                        }

                        LoginScreen(
                            viewModel = authViewModel,
                            onLoginSuccess = {
                                try {
                                    Log.d("AppNavigation", "Sinal de loginSuccessful recebido. Redirecionando a home.")
                                    navController.navigate("home") {
                                        popUpTo("login") { inclusive = true }
                                    }
                                } catch (e: Exception) {
                                    Log.e("AppNavigation", "Erro na transicao pos-login", e)
                                }
                            }
                        )
                    } catch (e: Exception) {
                        Log.e("AppNavigation", "Falha critica ao injetar ou exibir LoginScreen", e)
                    }
                }

                composable("home") {
                    try {
                        Log.d("AppNavigation", "NavHost -> Criando HomeScreen")
                        val homeViewModel: HomeViewModel = viewModel(factory = appContainer.homeViewModelFactory)
                        val realtimeViewModel = appContainer.realtimeViewModel
                        HomeScreen(
                            viewModel = homeViewModel,
                            realtimeViewModel = realtimeViewModel,
                            onNavigateToMonitoring = {
                                try {
                                    Log.d("AppNavigation", "Redirecionando de Home -> Monitoring")
                                    navController.navigate("monitoring")
                                } catch (e: Exception) {
                                    Log.e("AppNavigation", "Erro no clique home/monitoramento", e)
                                }
                            },
                        )
                    } catch (e: Exception) {
                        Log.e("AppNavigation", "Falha critica ao exibir HomeScreen", e)
                    }
                }

                composable("monitoring") {
                    try {
                        Log.d("AppNavigation", "NavHost -> Criando MonitoringScreen")
                        val monitoringViewModel: MonitoringViewModel = viewModel(factory = appContainer.monitoringViewModelFactory)
                        MonitoringScreen(
                            viewModel = monitoringViewModel,
                            onNavigateUp = { 
                                try {
                                    Log.d("AppNavigation", "Voltando de MonitoringScreen -> Home")
                                    navController.navigateUp() 
                                } catch (e: Exception) {
                                    Log.e("AppNavigation", "Erro ao fechar tela de monitoramento", e)
                                }
                            }
                        )
                    } catch (e: Exception) {
                        Log.e("AppNavigation", "Falha ao mostrar tela MonitoringScreen", e)
                    }
                }

                composable("profile") {
                    try {
                        Log.d("AppNavigation", "NavHost -> Criando ProfileScreen")
                        val profileViewModel: ProfileViewModel = viewModel(factory = appContainer.profileViewModelFactory)
                        ProfileScreen(
                            viewModel = profileViewModel,
                            onNavigateUp = { 
                                try {
                                    Log.d("AppNavigation", "Voltando de ProfileScreen -> Home")
                                    navController.navigateUp() 
                                } catch (e: Exception) {
                                    Log.e("AppNavigation", "Erro ao fechar Profile", e)
                                }
                            }
                        )
                    } catch (e: Exception) {
                        Log.e("AppNavigation", "Falha ao demonstrar ProfileScreen", e)
                    }
                }
            }
        }
    } catch (e: Exception) {
        Log.e("AppNavigation", "Falha terminal na estrutura do Scaffold(Host) Compose", e)
    }
}
