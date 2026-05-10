package br.edu.fatecpg.feature.home.components

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.ui.graphics.vector.ImageVector

object IconMapper {
    fun getIcon(name: String): ImageVector {
        return when (name.lowercase()) {
            "sensor", "cpu" -> Icons.Default.Hardware
            "cloud", "cloudupload" -> Icons.Default.Cloud
            "dashboard", "barchart" -> Icons.Default.Assessment
            "water", "flood" -> Icons.Default.WaterDrop
            else -> Icons.Default.Info
        }
    }
}