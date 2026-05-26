package br.edu.fatecpg.feature.home.ui

import android.Manifest
import android.os.Build
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import br.edu.fatecpg.feature.home.components.AlertCard
import br.edu.fatecpg.feature.home.components.StatCardItem
import br.edu.fatecpg.feature.home.viewmodel.HomeUiState
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel
import coil.compose.AsyncImage

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeScreen(
    viewModel: HomeViewModel,
    isLoggedIn: Boolean,
    onNavigateToLogin: () -> Unit,
    onOpenMapClick: (Double, Double, String) -> Unit
) {
    // Consumo seguro dos fluxos reativos
    val activeAlert by viewModel.activeAlert.collectAsStateWithLifecycle()
    val connectionError by viewModel.connectionError.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    val permissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        // Log ou tratamento opcional pode ser adicionado aqui
    }

    LaunchedEffect(Unit) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            permissionLauncher.launch(Manifest.permission.POST_NOTIFICATIONS)
        }
    }

    val sortedStats = androidx.compose.runtime.remember(uiState) {
        if (uiState is HomeUiState.Success) {
            (uiState as HomeUiState.Success).data.stats.sortedBy { it.order }
        } else {
            emptyList()
        }
    }

    val carouselsList = androidx.compose.runtime.remember(uiState) {
        if (uiState is HomeUiState.Success) {
            (uiState as HomeUiState.Success).data.carousels.sortedBy { it.order }
        } else {
            emptyList()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Visão Geral", fontWeight = FontWeight.Bold) },
                actions = {
                    if (isLoggedIn) {
                        IconButton(onClick = { viewModel.loadHomeContent() }) {
                            Icon(Icons.Default.Refresh, contentDescription = "Atualizar")
                        }
                    }
                }
            )
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            // 1. Visitante não logado
            if (!isLoggedIn) {
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp),
                    colors = CardDefaults.cardColors(
                        containerColor = MaterialTheme.colorScheme.primaryContainer
                    )
                ) {
                    Column(
                        modifier = Modifier.padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Text(
                            text = "Portal Operacional",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.Bold
                        )
                        Spacer(modifier = Modifier.height(8.dp))
                        Text(
                            text = "Faça login para acompanhar a telemetria dos bueiros e receber alertas em campo.",
                            textAlign = TextAlign.Center,
                            style = MaterialTheme.typography.bodyMedium
                        )
                        Spacer(modifier = Modifier.height(16.dp))
                        Button(onClick = onNavigateToLogin) {
                            Text("Fazer Login")
                        }
                    }
                }
                return@Box
            }

            // 2. Conteúdo Logado (LazyColumn para rolagem fluida)
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(horizontal = 16.dp),
                contentPadding = PaddingValues(bottom = 24.dp)
            ) {
                // Banner de Erro de Conexão WebSocket
                if (connectionError != null) {
                    item {
                        Card(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(bottom = 16.dp),
                            colors = CardDefaults.cardColors(
                                containerColor = MaterialTheme.colorScheme.errorContainer
                            ),
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

                // Card de Alerta Crítico em Tempo Real (Topo da prioridade)
                activeAlert?.let { alert ->
                    item {
                        AlertCard(
                            alert = alert,
                            onDismiss = { viewModel.dismissAlert() },
                            onOpenMapClick = onOpenMapClick
                        )
                        Spacer(modifier = Modifier.height(8.dp))
                    }
                }

                when (val state = uiState) {
                    is HomeUiState.Idle, is HomeUiState.Loading -> {
                        item {
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(32.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                CircularProgressIndicator()
                            }
                        }
                    }
                    is HomeUiState.Success -> {
                        if (carouselsList.isNotEmpty()) {
                            item {
                                val pagerState = rememberPagerState(pageCount = { carouselsList.size })

                                HorizontalPager(
                                    state = pagerState,
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .height(200.dp)
                                        .padding(vertical = 8.dp)
                                ) { page ->
                                    val carouselItem = carouselsList[page]
                                    Card(
                                        shape = RoundedCornerShape(12.dp),
                                        modifier = Modifier.fillMaxSize(),
                                        colors = CardDefaults.cardColors(
                                            containerColor = MaterialTheme.colorScheme.surfaceVariant
                                        )
                                    ) {
                                        AsyncImage(
                                            model = carouselItem.imageUrl,
                                            contentDescription = carouselItem.title,
                                            modifier = Modifier.fillMaxSize(),
                                            contentScale = ContentScale.Crop
                                        )
                                    }
                                }
                                Spacer(modifier = Modifier.height(8.dp))
                            }
                        }

                        // Seção de Estatísticas do Sistema (Dados do Room / HTTP)
                        item {
                            Text(
                                text = "Métricas da Malha",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold,
                                modifier = Modifier.padding(vertical = 8.dp)
                            )
                        }

                        if (sortedStats.isEmpty()) {
                            item {
                                Text(
                                    text = "Nenhuma métrica configurada no painel.",
                                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                                    style = MaterialTheme.typography.bodyMedium,
                                    modifier = Modifier.padding(vertical = 16.dp)
                                )
                            }
                        } else {
                            items(sortedStats.chunked(2), key = { pair -> pair.first().id }) { pair ->
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.spacedBy(12.dp)
                                ) {
                                    StatCardItem(
                                        stat = pair[0],
                                        modifier = Modifier.weight(1f)
                                    )
                                    if (pair.size > 1) {
                                        StatCardItem(
                                            stat = pair[1],
                                            modifier = Modifier.weight(1f)
                                        )
                                    } else {
                                        Spacer(modifier = Modifier.weight(1f))
                                    }
                                }
                                Spacer(modifier = Modifier.height(12.dp))
                            }
                        }
                    }
                    is HomeUiState.Error -> {
                        item {
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(
                                    containerColor = MaterialTheme.colorScheme.surfaceVariant
                                )
                            ) {
                                Column(
                                    modifier = Modifier.padding(16.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally
                                ) {
                                    Text(
                                        text = state.message,
                                        color = MaterialTheme.colorScheme.error,
                                        textAlign = TextAlign.Center,
                                        style = MaterialTheme.typography.bodyMedium
                                    )
                                    Spacer(modifier = Modifier.height(8.dp))
                                    TextButton(onClick = { viewModel.loadHomeContent() }) {
                                        Text("Tentar Novamente")
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}