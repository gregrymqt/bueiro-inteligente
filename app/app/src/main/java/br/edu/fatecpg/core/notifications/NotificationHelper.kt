package br.edu.fatecpg.core.notifications

import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import android.os.Build
import android.util.Log
import androidx.core.app.NotificationCompat
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO

class NotificationHelper(private val context: Context) {

    companion object {
        private const val CHANNEL_ID = "critical_drain_alerts"
        private const val CHANNEL_NAME = "Alertas Críticos de Bueiros"
    }

    init {
        createNotificationChannel()
    }

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val importance = NotificationManager.IMPORTANCE_HIGH
            val channel = NotificationChannel(CHANNEL_ID, CHANNEL_NAME, importance).apply {
                description = "Notificações para níveis críticos de obstrução em bueiros"
            }
            val notificationManager = context.getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager
            notificationManager.createNotificationChannel(channel)
        }
    }

    fun showCriticalNotification(drain: DrainStatusDTO) {
        try {
            val notificationManager = context.getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager
            
            val builder = NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.stat_sys_warning)
                .setContentTitle("🚨 Risco de Enchente - ${drain.name}")
                .setContentText("O bueiro atingiu ${drain.nivelObstrucao?.toInt() ?: 0}% de obstrução!")
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true)

            // Usa o hashCode do ID para evitar que bueiros diferentes se sobrescrevam
            val notificationId = drain.id.hashCode()
            notificationManager.notify(notificationId, builder.build())
        } catch (e: Exception) {
            Log.e("NotificationHelper", "Erro ao disparar notificação", e)
        }
    }
}
