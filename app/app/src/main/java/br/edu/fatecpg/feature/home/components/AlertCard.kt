package br.edu.fatecpg.feature.home.components

import android.content.Intent
import android.net.Uri
import android.util.Log
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Map
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO

@Composable
fun AlertCard(
    alert: DrainStatusDTO,
    onDismiss: () -> Unit
) {
    val context = LocalContext.current
    
    val cardColor = try {
        val statusLower = alert.status.lowercase()
        when (statusLower) {
            "crítico", "critico" -> Color(0xFFFFCDD2) // Vermelho suave
            "alerta" -> Color(0xFFFFE082) // Amarelo suave
            else -> MaterialTheme.colorScheme.surfaceVariant
        }
    } catch (e: Exception) {
        Log.e("AlertCard", "Erro ao processar cor do status", e)
        MaterialTheme.colorScheme.surfaceVariant
    }

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(16.dp),
        colors = CardDefaults.cardColors(containerColor = cardColor),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        try {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp)
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Bueiro ID: ${alert.idBueiro}",
                        fontWeight = FontWeight.Bold,
                        style = MaterialTheme.typography.titleMedium,
                        color = Color.Black
                    )
                    IconButton(onClick = {
                        try {
                            onDismiss()
                        } catch (e: Exception) {
                            Log.e("AlertCard", "Erro ao dispensar alerta", e)
                        }
                    }, modifier = Modifier.size(24.dp)) {
                        Icon(
                            imageVector = Icons.Default.Close,
                            contentDescription = "Fechar Alerta",
                            tint = Color.Black
                        )
                    }
                }

                Spacer(modifier = Modifier.height(8.dp))

                Text(
                    text = "Atualizado às: ${alert.ultimaAtualizacao}",
                    style = MaterialTheme.typography.bodySmall,
                    color = Color.DarkGray
                )

                Spacer(modifier = Modifier.height(16.dp))

                val level = alert.nivelObstrucao
                Text(
                    text = "Obstrução: ${level.toInt()}%",
                    style = MaterialTheme.typography.bodyMedium,
                    fontWeight = FontWeight.SemiBold,
                    color = Color.Black
                )

                Spacer(modifier = Modifier.height(4.dp))

                LinearProgressIndicator(
                    progress = { (level.toFloat() / 100f) },
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(8.dp),
                    color = if (level >= 80) Color.Red else Color.DarkGray,
                    trackColor = Color.White
                )

                Spacer(modifier = Modifier.height(16.dp))

                Button(
                    onClick = {
                        try {
                            Log.d("AlertCard", "Iniciando intent para Google Maps")
                            val lat = alert.latitude ?: 0.0
                            val lng = alert.longitude ?: 0.0
                            val uri = "geo:$lat,$lng?q=$lat,$lng(Bueiro+${alert.idBueiro})"
                            val intent = Intent(Intent.ACTION_VIEW, Uri.parse(uri))
                            intent.setPackage("com.google.android.apps.maps")
                            context.startActivity(intent)
                        } catch (e: Exception) {
                            Log.e("AlertCard", "Erro ao tentar abrir mapa externo", e)
                        }
                    },
                    modifier = Modifier.fillMaxWidth(),
                    colors = ButtonDefaults.buttonColors(containerColor = Color.Black)
                ) {
                    Icon(imageVector = Icons.Default.Map, contentDescription = "Mapa", modifier = Modifier.size(18.dp))
                    Spacer(modifier = Modifier.width(8.dp))
                    Text(text = "Abrir no Mapa", color = Color.White)
                }
            }
        } catch (e: Exception) {
            Log.e("AlertCard", "Erro geral na renderizacao do card", e)
        }
    }
}
