package br.edu.fatecpg.feature.monitoring.ui

import android.util.Log
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.compose.runtime.getValue
import androidx.compose.ui.text.style.TextAlign
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringUiState
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MonitoringScreen(
    viewModel: MonitoringViewModel,
    isLoggedIn: Boolean,
    onNavigateToLogin: () -> Unit
) {
        val uiState by viewModel.uiState.collectAsStateWithLifecycle()
        val showLoginDialog by viewModel.showLoginDialog.collectAsStateWithLifecycle()

        Scaffold(
            floatingActionButton = {
                FloatingActionButton(onClick = {
                    viewModel.refreshDrains()
                }) {
                    Icon(imageVector = Icons.Default.Refresh, contentDescription = "Atualizar")
                }
            }
        ) { paddingValues ->
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(paddingValues)
            ) {
                when (uiState) {
                    is MonitoringUiState.Loading -> {
                        CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                    }
                    is MonitoringUiState.Success -> {
                        val drains = (uiState as MonitoringUiState.Success).drains
                        
                        if (drains.isEmpty()) {
                            Text(
                                "Nenhum bueiro cadastrado no momento.",
                                modifier = Modifier.align(Alignment.Center)
                            )
                        } else {
                            LazyColumn(
                                modifier = Modifier.fillMaxSize(),
                                contentPadding = PaddingValues(16.dp),
                                verticalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                items(drains) { drain ->
                                    DrainItemCard(
                                        drain = drain,
                                        onClick = {
                                            viewModel.onDrainClick(isLoggedIn, drain)
                                        }
                                    )
                                }
                            }
                        }
                    }
                    is MonitoringUiState.Error -> {
                        Column(
                            modifier = Modifier.align(Alignment.Center),
                            horizontalAlignment = Alignment.CenterHorizontally
                        ) {
                            Text(
                                text = (uiState as MonitoringUiState.Error).message,
                                color = MaterialTheme.colorScheme.error,
                                textAlign = TextAlign.Center,
                                modifier = Modifier.padding(16.dp)
                            )
                            Button(onClick = {
                                viewModel.refreshDrains()
                            }) {
                                Text("Tentar Novamente")
                            }
                        }
                    }
                }

                if (showLoginDialog) {
                    AlertDialog(
                        onDismissRequest = {
                            viewModel.dismissLoginDialog()
                        },
                        title = { Text("Acesso Restrito") },
                        text = { Text("Para ver a localizaзгo exata e detalhes do bueiro, й necessбrio estar logado.") },
                        confirmButton = {
                            TextButton(onClick = {
                                viewModel.dismissLoginDialog()
                                onNavigateToLogin()
                            }) {
                                Text("Fazer Login")
                            }
                        },
                        dismissButton = {
                            TextButton(onClick = {
                                viewModel.dismissLoginDialog()
                            }) {
                                Text("Cancelar")
                            }
                        }
                    )
                }
            }
        }
}

@Composable
fun DrainItemCard(drain: DrainStatusDTO, onClick: () -> Unit) {
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .clickable { onClick() },
            elevation = CardDefaults.cardElevation(defaultElevation = 2.dp),
            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
        ) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                // Indicador de Status (Bolinha colorida)
                Box(
                    modifier = Modifier
                        .size(16.dp)
                        .background(
                            color = Color(MonitoringViewModel.getStatusColor(drain.status)),
                            shape = CircleShape
                        )
                )

                Spacer(modifier = Modifier.width(16.dp))

                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = "Bueiro ${drain.idBueiro}",
                        fontWeight = FontWeight.Bold,
                        fontSize = 16.sp
                    )
                    Text(
                        text = "Status: ${drain.status.uppercase()}",
                        fontSize = 14.sp,
                        color = Color.DarkGray
                    )
                    Text(
                        text = "Obstruзгo: ${drain.nivelObstrucao.toInt()}%",
                        fontSize = 14.sp,
                        color = Color.Gray
                    )
                }
            }
        }
}
