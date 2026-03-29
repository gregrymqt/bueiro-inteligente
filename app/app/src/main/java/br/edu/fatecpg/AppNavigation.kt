package br.edu.fatecpg

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import br.edu.fatecpg.core.di.AppContainer
import br.edu.fatecpg.core.navigation.BottomNavRoutes
import br.edu.fatecpg.core.navigation.MainBottomBar
import br.edu.fatecpg.feature.auth.ui.LoginScreen
import br.edu.fatecpg.feature.auth.viewmodel.LoginViewModel
import br.edu.fatecpg.feature.auth.viewmodel.LoginViewModelFactory
import br.edu.fatecpg.feature.home.ui.HomeScreen
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModelFactory
import br.edu.fatecpg.feature.monitoring.ui.MonitoringScreen
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModelFactory
import br.edu.fatecpg.feature.profile.ui.ProfileScreen
import br.edu.fatecpg.feature.profile.viewmodel.ProfileViewModel
import br.edu.fatecpg.feature.profile.viewmodel.ProfileViewModelFactory
import kotlinx.coroutines.launch

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppNavigation(appContainer: AppContainer) {
    val navController = rememberNavController()
    
    val isLoggedIn by appContainer.tokenManager.isLoggedIn.collectAsStateWithLifecycle()
    
    // Rota inicial dependendo de ter token salvo ou não (calculada na largada)
    val startDestination = remember {
        if (appContainer.tokenManager.getToken().isNullOrEmpty()) BottomNavRoutes.Login.route else BottomNavRoutes.Home.route
    }

    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    // Oculta bottom bar na tela de login
    val showBottomBar = currentRoute != BottomNavRoutes.Login.route

    val coroutineScope = rememberCoroutineScope()

    Scaffold(
        topBar = {
            if (showBottomBar) {
                TopAppBar(
                    title = { Text(text = "Bueiro Inteligente") },
                    actions = {
                        IconButton(onClick = {
                            coroutineScope.launch {
                                appContainer.authRepository.logout()
                                navController.navigate(BottomNavRoutes.Login.route) {
                                    popUpTo(0) { inclusive = true } // Limpa a pilha de navegação
                                }
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
                MainBottomBar(navController = navController, isLoggedIn = isLoggedIn)
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = startDestination,
            modifier = Modifier.padding(innerPadding)
        ) {
            composable(BottomNavRoutes.Login.route) {
                val viewModel: LoginViewModel = viewModel(
                    factory = LoginViewModelFactory(appContainer.authRepository)
                )
                
                LoginScreen(
                    viewModel = viewModel,
                    onNavigateToHome = {
                        navController.navigate(BottomNavRoutes.Home.route) {
                            popUpTo(BottomNavRoutes.Login.route) { inclusive = true }
                        }
                    }
                )
            }
            
            composable(BottomNavRoutes.Home.route) {
                val viewModel: HomeViewModel = viewModel(
                    factory = HomeViewModelFactory(appContainer.realtimeRepository, appContainer.tokenManager)
                )
                HomeScreen(
                    viewModel = viewModel,
                    isLoggedIn = isLoggedIn,
                    onNavigateToLogin = {
                        navController.navigate(BottomNavRoutes.Login.route) {
                            popUpTo(BottomNavRoutes.Home.route) { inclusive = true }
                        }
                    }
                )
            }
            
            composable(BottomNavRoutes.Monitoring.route) {
                val viewModel: MonitoringViewModel = viewModel(
                    factory = MonitoringViewModelFactory(appContainer.monitoringRepository, appContainer.locationHandler)
                )
                
                MonitoringScreen(
                    viewModel = viewModel,
                    isLoggedIn = isLoggedIn,
                    onNavigateToLogin = {
                        navController.navigate(BottomNavRoutes.Login.route) {
                            popUpTo(BottomNavRoutes.Home.route) { saveState = true }
                        }
                    }
                )
            }
            
            composable(BottomNavRoutes.Profile.route) {
                val profileScope = rememberCoroutineScope()
                
                val profileViewModel: ProfileViewModel = viewModel(
                    factory = ProfileViewModelFactory(appContainer.profileRepository)
                )
                
                ProfileScreen(
                    viewModel = profileViewModel,
                    onLogoutClick = {
                        profileScope.launch {
                            appContainer.authRepository.logout()
                            navController.navigate(BottomNavRoutes.Login.route) {
                                popUpTo(navController.graph.findStartDestination().id) { inclusive = true }
                            }
                        }
                    }
                )
            }
        }
    }
}
