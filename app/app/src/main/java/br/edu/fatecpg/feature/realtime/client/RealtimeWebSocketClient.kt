package br.edu.fatecpg.feature.realtime.client

import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import com.google.gson.Gson
import com.google.gson.annotations.SerializedName
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.Response
import okhttp3.WebSocket
import okhttp3.WebSocketListener

class RealtimeWebSocketClient(
    private val okHttpClient: OkHttpClient,
    private val gson: Gson
) {
    private var webSocket: WebSocket? = null
    private val coroutineScope = CoroutineScope(Dispatchers.IO)

    private val _drainStatusFlow = MutableSharedFlow<DrainStatusDTO>()
    val drainStatusFlow: SharedFlow<DrainStatusDTO> = _drainStatusFlow.asSharedFlow()

    fun connect() {
        val request = Request.Builder()
            .url("ws://10.0.2.2:8000/realtime/ws")
            .build()
        webSocket = okHttpClient.newWebSocket(request, DrainWebSocketListener())
    }

    fun disconnect() {
        webSocket?.close(1000, "App closed")
        webSocket = null
    }

    private inner class DrainWebSocketListener : WebSocketListener() {
        override fun onMessage(webSocket: WebSocket, text: String) {
            try {
                val message = gson.fromJson(text, RealtimeMessage::class.java)
                if (message.eventoTipo == "BUEIRO_STATUS_MUDOU" && message.dados != null) {
                    val status = gson.fromJson(gson.toJson(message.dados), DrainStatusDTO::class.java)
                    coroutineScope.launch {
                        _drainStatusFlow.emit(status)
                    }
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }

        override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
            t.printStackTrace()
        }
    }

    private data class RealtimeMessage(
        @SerializedName("evento_tipo") val eventoTipo: String,
        @SerializedName("dados") val dados: Any?
    )
}
