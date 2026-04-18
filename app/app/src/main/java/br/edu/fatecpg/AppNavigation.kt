package br.edu.fatecpg

import android.util.Log
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.*
import br.edu.fatecpg.core.di.AppContainer
import br.edu.fatecpg.core.navigation.MainBottomBar
import br.edu.fatecpg.feature.auth.ui.LoginScreen
import br.edu.fatecpg.feature.auth.ui.RegisterScreen
import br.edu.fatecpg.feature.auth.viewmodel.LoginViewModel
import br.edu.fatecpg.feature.auth.viewmodel.RegisterViewModel
import br.edu.fatecpg.feature.home.ui.HomeScreen
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel
import br.edu.fatecpg.feature.monitoring.ui.MonitoringScreen
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel
import br.edu.fatecpg.feature.profile.ui.ProfileScreen
import br.edu.fatecpg.feature.profile.viewmodel.ProfileViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppNavigation(appContainer: AppContainer) {
    val navController = rememberNavController()
    val isLoggedIn = appContainer.tokenManager.getToken() != null
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    val onLogout: () -> Unit = {
        try {
            Log.i("AppNavigation", "Executando logout...")
            appContainer.tokenManager.clearToken()
            navController.navigate("login") {
                popUpTo(0) { inclusive = true }
            }
        } catch (e: Exception) {
            Log.e("AppNavigation", "Erro ao executar logout", e)
        }
    }

    Scaffold(
        topBar = {
            Log.d("AppNavigation", "Redesenhando TopBar. Rota atual: $currentRoute")

            if (currentRoute != "login" && currentRoute != "register") {
                TopAppBar(
                    title = { Text(text = "Bueiro Inteligente", fontWeight = FontWeight.Bold) },
                    actions = {
                        IconButton(onClick = onLogout) {
                            Icon(Icons.AutoMirrored.Filled.ExitToApp, contentDescription = "Sair")
                        }
                    },
                    colors = TopAppBarDefaults.topAppBarColors(
                        containerColor = MaterialTheme.colorScheme.primary,
                        titleContentColor = MaterialTheme.colorScheme.onPrimary,
                        actionIconContentColor = MaterialTheme.colorScheme.onPrimary
                    )
                )
            }
        },
        bottomBar = {
            if (isLoggedIn && currentRoute != "login" && currentRoute != "register") {
                MainBottomBar(navController = navController, isLoggedIn = true)
            }
        }
    ) { paddingValues ->
        NavHost(
            navController = navController,
            startDestination = "home",
            modifier = Modifier.padding(paddingValues)
        ) {
            composable("login") {
                Log.d("AppNavigation", "NavHost -> Criando LoginScreen")
                val loginViewModel: LoginViewModel = viewModel(factory = appContainer.authViewModelFactory)

                LaunchedEffect(Unit) {
                    if (appContainer.tokenManager.getToken() != null) {
                        Log.i("AppNavigation", "Token encontrado. Redirecionando para home.")
                        navController.navigate("home") {
                            popUpTo("login") { inclusive = true }
                        }
                    }
                }

                LoginScreen(
                    viewModel = loginViewModel,
                    onNavigateToHome = {
                        navController.navigate("home") {
                            popUpTo("login") { inclusive = true }
                        }
                    },
                    onNavigateToRegister = {
                        navController.navigate("register")
                    }
                )
            }

            composable("register") {
                Log.d("AppNavigation", "NavHost -> Criando RegisterScreen")
                val registerViewModel: RegisterViewModel = viewModel(factory = appContainer.registerViewModelFactory)

                RegisterScreen(
                    viewModel = registerViewModel,
                    onNavigateBackToLogin = {
                        navController.popBackStack("login", inclusive = false)
                    },
                    onRegisterSuccess = {
                        navController.popBackStack("login", inclusive = false)
                    }
                )
            }

            composable("home") {
                Log.d("AppNavigation", "NavHost -> Criando HomeScreen")
                val homeViewModel: HomeViewModel = viewModel(factory = appContainer.homeViewModelFactory)

                HomeScreen(
                    viewModel = homeViewModel,
                    isLoggedIn = isLoggedIn,
                    onNavigateToLogin = {
                        navController.navigate("login")
                    }
                )
            }

            composable("monitoring") {
                Log.d("AppNavigation", "NavHost -> Criando MonitoringScreen")
                val monitoringViewModel: MonitoringViewModel = viewModel(factory = appContainer.monitoringViewModelFactory)

                MonitoringScreen(
                    viewModel = monitoringViewModel,
                    isLoggedIn = isLoggedIn,
                    onNavigateToLogin = {
                        navController.navigate("login")
                    }
                )
            }

            composable("profile") {
                Log.d("AppNavigation", "NavHost -> Criando ProfileScreen")
                val profileViewModel: ProfileViewModel = viewModel(factory = appContainer.profileViewModelFactory)
                ProfileScreen(
                    viewModel = profileViewModel,
                    onLogoutClick = onLogout
                )
            }
        }
    }
}
