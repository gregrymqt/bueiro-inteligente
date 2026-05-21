package br.edu.fatecpg.core.notifications

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage
import com.google.gson.Gson

class BueiroFirebaseMessagingService : FirebaseMessagingService() {

    override fun onMessageReceived(remoteMessage: RemoteMessage) {
        super.onMessageReceived(remoteMessage)

        if (remoteMessage.data.isNotEmpty()) {
            val data = remoteMessage.data
            Log.d(TAG, "Payload de dados recebido: $data")

            try {
                // Reconstrução manual do DTO garantindo a conversão de tipos
                val drain = DrainStatusDTO(
                    id = data["id"] ?: "",
                    name = data["name"] ?: "Bueiro sem nome",
                    address = data["address"] ?: "",
                    hardwareId = data["hardware_id"] ?: "",
                    isActive = data["is_active"]?.toBoolean() ?: true,
                    status = data["status"],
                    nivelObstrucao = data["nivel_obstrucao"]?.toDoubleOrNull(),
                    distanciaCm = data["distancia_cm"]?.toDoubleOrNull(),
                    ultimaAtualizacao = data["ultima_atualizacao"],
                    latitude = data["latitude"]?.toDoubleOrNull(),
                    longitude = data["longitude"]?.toDoubleOrNull()
                )

                // Validação defensiva para status crítico
                val status = drain.status?.lowercase()
                if (status == "crítico" || status == "critico") {
                    val notificationHelper = NotificationHelper(applicationContext)
                    notificationHelper.showCriticalNotification(drain)
                }
            } catch (e: Exception) {
                Log.e(TAG, "Erro ao processar dados da notificação", e)
            }
        }
    }

    override fun onNewToken(token: String) {
        super.onNewToken(token)
        Log.d(TAG, "Novo token gerado pelo Firebase: $token")
        
        // TODO: Enviar token para o backend via TokenManager
    }

    companion object {
        private const val TAG = "BueiroFCMService"
    }
}
