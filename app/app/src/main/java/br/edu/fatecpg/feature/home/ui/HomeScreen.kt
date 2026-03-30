package br.edu.fatecpg.feature.home.ui

import android.util.Log
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import br.edu.fatecpg.feature.home.components.AlertCard
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel

@Composable
fun HomeScreen(
    viewModel: HomeViewModel,
    isLoggedIn: Boolean,
    onNavigateToLogin: () -> Unit
) {
        val activeAlert by viewModel.activeAlert.collectAsStateWithLifecycle()
        val connectionError by viewModel.connectionError.collectAsStateWithLifecycle()

        Column(modifier = Modifier.fillMaxSize()) {
            if (!isLoggedIn) {
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer),
                    elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
                ) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Text(
                            text = "Bem-vindo ao Bueiro Inteligente! Faça login para ter acesso completo ao monitoramento e alertas.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onPrimaryContainer,
                            textAlign = TextAlign.Center
                        )
                        Spacer(modifier = Modifier.height(12.dp))
                        Button(onClick = {
                            Log.d("HomeScreen", "Navegando para o Login via botão")
                            onNavigateToLogin()
                        }) {
                            Text("Entrar")
                        }
                    }
                }
            }

            Box(modifier = Modifier.weight(1f)) {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(16.dp),
                    verticalArrangement = Arrangement.Center,
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    if (activeAlert != null && isLoggedIn) {
                        AlertCard(
                            alert = activeAlert!!,
                            onDismiss = {
                                viewModel.dismissAlert()
                            }
                        )
                    } else if (isLoggedIn) {
                        Text(
                            text = "Nenhum alerta crítico no momento.\nTudo tranquilo por aqui! \uD83C\uDF3F",
                            style = MaterialTheme.typography.bodyLarge,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center
                        )
                    } else {
                        // Visitantes sem dados restritos
                        Text(
                            text = "O monitoramento contínuo ajuda a prevenir enchentes urbanas.",
                            style = MaterialTheme.typography.bodyLarge,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center
                        )
                    }
                }
                
                if (connectionError != null && isLoggedIn) {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .align(Alignment.TopCenter)
                            .padding(16.dp),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.errorContainer),
                        shape = RoundedCornerShape(8.dp)
                    ) {
                        Text(
                            text = connectionError ?: "Erro de conexão com os sensores.",
                            modifier = Modifier.padding(16.dp),
                            color = MaterialTheme.colorScheme.onErrorContainer,
                            style = MaterialTheme.typography.bodyMedium,
                            fontWeight = FontWeight.Bold,
                            textAlign = TextAlign.Center
                        )
                    }
                }
            }
        }
}
