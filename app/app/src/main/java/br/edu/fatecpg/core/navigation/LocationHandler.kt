package br.edu.fatecpg.core.navigation

import android.content.Context
import android.content.Intent
import android.net.Uri
import android.widget.Toast

interface LocationHandler {
    fun openLocation(latitude: Double, longitude: Double, label: String)
}

class AndroidLocationHandler(private val context: Context) : LocationHandler {
    override fun openLocation(latitude: Double, longitude: Double, label: String) {
        val uri = Uri.parse("geo:${latitude},${longitude}?q=${latitude},${longitude}($label)")
        val mapIntent = Intent(Intent.ACTION_VIEW, uri)
        mapIntent.setPackage("com.google.android.apps.maps")
        
        try {
            if (mapIntent.resolveActivity(context.packageManager) != null) {
                context.startActivity(mapIntent)
            } else {
                val browserIntent = Intent(Intent.ACTION_VIEW, Uri.parse("https://maps.google.com/?q=${latitude},${longitude}"))
                context.startActivity(browserIntent)
            }
        } catch (e: Exception) {
            Toast.makeText(context, "Erro ao abrir o mapa", Toast.LENGTH_SHORT).show()
        }
    }
}
