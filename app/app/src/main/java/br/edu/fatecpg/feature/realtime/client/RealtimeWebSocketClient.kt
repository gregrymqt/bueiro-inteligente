package br.edu.fatecpg.feature.realtime.client

import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import com.google.gson.Gson
import com.google.gson.JsonElement
import com.google.gson.annotations.SerializedName
import kotlinx.coroutines.channels.BufferOverflow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
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

    // SharedFlow para publicar as atualizações de forma reativa para as ViewModels.
    // Usamos um buffer para garantir que eventos próximos não sejam perdidos caso o coletor esteja ocupado.
    private val _drainStatusUpdates = MutableSharedFlow<DrainStatusDTO>(
        extraBufferCapacity = 10,
        onBufferOverflow = BufferOverflow.DROP_OLDEST
    )
    val drainStatusUpdates: SharedFlow<DrainStatusDTO> = _drainStatusUpdates.asSharedFlow()

    fun connect(token: String?) {
        // ws:// ou wss:// dependendo do ambiente. Usando o padrão de emulador apontando pro host.
        val baseUrl = "ws://10.0.2.2:8000/realtime/ws"
        
        val requestBuilder = Request.Builder().url(baseUrl)
        
        // Se a sua API validar autenticação no socket baseando-se em Header, passamos desta forma:
        if (!token.isNullOrEmpty()) {
            requestBuilder.addHeader("Authorization", "Bearer $token")
        }

        webSocket = okHttpClient.newWebSocket(requestBuilder.build(), createWebSocketListener())
    }

    fun disconnect() {
        webSocket?.close(1000, "Client disconnected")
        webSocket = null
    }

    private fun createWebSocketListener(): WebSocketListener {
        return object : WebSocketListener() {
            override fun onMessage(webSocket: WebSocket, text: String) {
                try {
                    // Desserializa a mensagem para identificar o tipo do evento
                    val WsMessage = gson.fromJson(text, RealtimeMessageWrapper::class.java)
                    
                    if (WsMessage.type == "BUEIRO_STATUS_MUDOU" && WsMessage.data != null) {
                        // Converte o payload JSON (data) para o nosso DTO existente da feature de monitoring
                        val drainStatus = gson.fromJson(WsMessage.data, DrainStatusDTO::class.java)
                        
                        // tryEmit emite o dado para os coletores de maneira segura e não bloqueante
                        _drainStatusUpdates.tryEmit(drainStatus)
                    }
                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }

            override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
                super.onFailure(webSocket, t, response)
                t.printStackTrace()
            }

            override fun onClosed(webSocket: WebSocket, code: Int, reason: String) {
                super.onClosed(webSocket, code, reason)
                println("WebSocket Closed: $code / $reason")
            }
        }
    }

    // Estrutura auxiliar esperada do BroadcastService no Backend (Python)
    private data class RealtimeMessageWrapper(
        @SerializedName("type") val type: String,
        @SerializedName("data") val data: JsonElement?
    )
}
