package br.edu.fatecpg.feature.monitoring.ui

import android.content.Context
import android.content.Intent
import android.net.Uri
import android.widget.Toast
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringUiState
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel
import com.google.accompanist.swiperefresh.SwipeRefresh
import com.google.accompanist.swiperefresh.rememberSwipeRefreshState
import java.text.SimpleDateFormat
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MonitoringScreen(
    viewModel: MonitoringViewModel
) {
    val uiState by viewModel.uiState.collectAsState()
    val isRefreshing = uiState is MonitoringUiState.Loading
    val swipeRefreshState = rememberSwipeRefreshState(isRefreshing)
    val context = LocalContext.current

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Monitoramento", fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        }
    ) { paddingValues ->
        SwipeRefresh(
            state = swipeRefreshState,
            onRefresh = { viewModel.refreshDrains() },
            modifier = Modifier.padding(paddingValues)
        ) {
            Box(modifier = Modifier.fillMaxSize()) {
                when (val state = uiState) {
                    is MonitoringUiState.Loading -> {
                        // SwipeRefresh já fornece o feedback visual de loading
                    }
                    is MonitoringUiState.Error -> {
                        Box(modifier = Modifier.fillMaxSize().androidx.compose.foundation.verticalScroll(androidx.compose.foundation.rememberScrollState())) {
                            ErrorState(
                                message = state.message,
                                onRetry = { viewModel.refreshDrains() }
                            )
                        }
                    }
                    is MonitoringUiState.Success -> {
                        if (state.drains.isEmpty()) {
                            Box(modifier = Modifier.fillMaxSize().androidx.compose.foundation.verticalScroll(androidx.compose.foundation.rememberScrollState())) {
                                EmptyState()
                            }
                        } else {
                            LazyColumn(
                                contentPadding = PaddingValues(16.dp),
                                verticalArrangement = Arrangement.spacedBy(12.dp),
                                modifier = Modifier.fillMaxSize()
                            ) {
                                items(state.drains) { drain ->
                                    DrainItem(
                                        drain = drain,
                                        onClick = { openGoogleMaps(context, drain) }
                                    )
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

private fun openGoogleMaps(context: Context, drain: DrainStatusDTO) {
    if (drain.latitude != null && drain.longitude != null) {
        val uri = Uri.parse("geo:${drain.latitude},${drain.longitude}?q=${drain.latitude},${drain.longitude}(Bueiro+${drain.idBueiro})")
        val mapIntent = Intent(Intent.ACTION_VIEW, uri)
        mapIntent.setPackage("com.google.android.apps.maps")
        if (mapIntent.resolveActivity(context.packageManager) != null) {
            context.startActivity(mapIntent)
        } else {
            // Fallback para abrir no navegador se o Google Maps não estiver instalado
            val browserIntent = Intent(Intent.ACTION_VIEW, Uri.parse("https://www.google.com/maps/search/?api=1&query=${drain.latitude},${drain.longitude}"))
            context.startActivity(browserIntent)
        }
    } else {
        Toast.makeText(context, "Localização indisponível para este bueiro", Toast.LENGTH_SHORT).show()
    }
}

@Composable
fun DrainItem(drain: DrainStatusDTO, onClick: () -> Unit) {
    // Carrega a cor através do Mapeador da ViewModel
    val statusColor = Color(MonitoringViewModel.getStatusColor(drain.status))
    
    // Converte a porcentagem de obstrução para um valor fracionário (0.0 até 1.0) para a barra de progresso
    val obstructionFraction = (drain.nivelObstrucao / 100f).coerceIn(0.0, 1.0).toFloat()

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
            // Indicador visual de cor em formato de círculo
            Canvas(modifier = Modifier.size(16.dp)) {
                drawCircle(color = statusColor)
            }

            Spacer(modifier = Modifier.width(16.dp))

            Column(modifier = Modifier.weight(1f)) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Bueiro: ${drain.idBueiro}",
                        fontWeight = FontWeight.Bold,
                        fontSize = 16.sp
                    )
                    Text(
                        text = formatDateTime(drain.ultimaAtualizacao),
                        fontSize = 12.sp,
                        color = Color.Gray
                    )
                }

                Spacer(modifier = Modifier.height(8.dp))

                Text(
                    text = "Obstrução: ${drain.nivelObstrucao.toInt()}%",
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Medium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )

                Spacer(modifier = Modifier.height(6.dp))

                // Barra de progresso baseada no nível de obstrução e com cor do status
                LinearProgressIndicator(
                    progress = obstructionFraction,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(8.dp),
                    color = statusColor,
                    trackColor = statusColor.copy(alpha = 0.2f)
                )
            }
        }
    }
}

@Composable
fun ErrorState(message: String, onRetry: () -> Unit) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Icon(
            imageVector = Icons.Default.Warning,
            contentDescription = "Erro",
            tint = MaterialTheme.colorScheme.error,
            modifier = Modifier.size(48.dp)
        )
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = message,
            color = MaterialTheme.colorScheme.error,
            textAlign = TextAlign.Center,
            fontSize = 16.sp
        )
        Spacer(modifier = Modifier.height(24.dp))
        Button(onClick = onRetry) {
            Text("Tentar Novamente")
        }
    }
}

@Composable
fun EmptyState() {
    Box(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = "Nenhum bueiro cadastrado ou disponível no momento.",
            color = Color.Gray,
            textAlign = TextAlign.Center,
            fontSize = 16.sp
        )
    }
}

// Utilitário para formatar a data que vem no padrão string ISO/Date-Time do servidor
private fun formatDateTime(isoString: String): String {
    return try {
        // Tenta padronizar o Timestamp (pode variar de acordo com o retorno real da API)
        val inputFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault())
        val outputFormat = SimpleDateFormat("dd/MM/yyyy HH:mm", Locale.getDefault())
        val date = inputFormat.parse(isoString)
        if (date != null) outputFormat.format(date) else isoString
    } catch (e: Exception) {
        // Se a string já vier formatada diferente, não exibe erro, passa a original
        isoString
    }
}