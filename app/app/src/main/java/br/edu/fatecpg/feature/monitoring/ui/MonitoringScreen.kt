package br.edu.fatecpg.feature.monitoring.ui

import android.util.Log
import androidx.compose.animation.animateContentSize
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.LocationOn
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
        val expandedDrainId by viewModel.expandedDrainId.collectAsStateWithLifecycle()

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
                                items(
                                    items = drains,
                                    key = { drain -> drain.id }
                                ) { drain ->
                                    val isExpanded = expandedDrainId == drain.id
                                    
                                    DrainItemCard(
                                        drain = drain,
                                        isExpanded = isExpanded,
                                        onClick = { viewModel.onDrainClick(isLoggedIn, drain) },
                                        onOpenInMaps = { viewModel.openDrainInMaps(drain) }
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
                        text = { Text("Para ver a localização exata e detalhes do bueiro, é necessário estar logado.") },
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
fun DrainItemCard(
    drain: DrainStatusDTO, 
    isExpanded: Boolean, 
    onClick: () -> Unit,
    onOpenInMaps: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable(onClick = onClick)
            .animateContentSize(),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp)
        ) {
            Row(
                verticalAlignment = Alignment.CenterVertically
            ) {
                val statusColor = Color(MonitoringViewModel.getStatusColor(drain.status))

                // Indicador de Status (Bolinha colorida)
                Box(
                    modifier = Modifier
                        .size(16.dp)
                        .background(
                            color = statusColor,
                            shape = CircleShape
                        )
                )

                Spacer(modifier = Modifier.width(16.dp))

                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = drain.name,
                        fontWeight = FontWeight.Bold,
                        fontSize = 16.sp
                    )
                    Text(
                        text = drain.address,
                        fontSize = 12.sp,
                        color = Color.Gray,
                        lineHeight = 16.sp
                    )
                }
            }

            if (isExpanded) {
                Spacer(modifier = Modifier.height(16.dp))
                HorizontalDivider(modifier = Modifier.padding(vertical = 8.dp), thickness = 0.5.dp, color = Color.LightGray)
                
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Column {
                        Text(
                            text = "Status: ${drain.status ?: "Desconhecido"}",
                            fontSize = 14.sp,
                            fontWeight = FontWeight.SemiBold
                        )
                        Text(
                            text = "Obstrução: ${drain.nivelObstrucao?.toInt() ?: 0}%",
                            fontSize = 14.sp,
                            color = if ((drain.nivelObstrucao ?: 0.0) > 70) MaterialTheme.colorScheme.error else Color.DarkGray
                        )
                        Text(
                            text = "Distância: ${drain.distanciaCm?.toInt() ?: 0} cm",
                            fontSize = 14.sp,
                            color = Color.DarkGray
                        )
                    }
                    
                    Column(horizontalAlignment = Alignment.End) {
                        Text(
                            text = "Última atualização:",
                            fontSize = 10.sp,
                            color = Color.Gray
                        )
                        Text(
                            text = drain.ultimaAtualizacao ?: "--:--",
                            fontSize = 12.sp,
                            color = Color.DarkGray
                        )
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))

                Button(
                    onClick = onOpenInMaps,
                    modifier = Modifier.fillMaxWidth(),
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primaryContainer, contentColor = MaterialTheme.colorScheme.onPrimaryContainer)
                ) {
                    Icon(imageVector = Icons.Default.LocationOn, contentDescription = null)
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Abrir no Google Maps")
                }
            } else {
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "Status: ${drain.status ?: "Desconhecido"} • Obstrução: ${drain.nivelObstrucao?.toInt() ?: 0}%",
                    fontSize = 13.sp,
                    color = Color.DarkGray
                )
            }
        }
    }
}
