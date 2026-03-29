package br.edu.fatecpg.core.navigation

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.List
import androidx.compose.material.icons.filled.Login
import androidx.compose.material.icons.filled.Person
import androidx.compose.ui.graphics.vector.ImageVector

sealed class BottomNavRoutes(val route: String, val title: String, val icon: ImageVector) {
    object Home : BottomNavRoutes("home", "Home", Icons.Default.Home)
    object Monitoring : BottomNavRoutes("monitoring", "Bueiros", Icons.Default.List)
    object Profile : BottomNavRoutes("profile", "Perfil", Icons.Default.Person)
    object Login : BottomNavRoutes("login", "Entrar", Icons.Default.Login)
}
