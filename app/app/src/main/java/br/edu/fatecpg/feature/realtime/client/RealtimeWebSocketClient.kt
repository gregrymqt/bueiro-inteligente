package br.edu.fatecpg.feature.realtime.client

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import com.google.gson.Gson
import com.google.gson.JsonElement
import com.google.gson.annotations.SerializedName
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.Response
import okhttp3.WebSocket
import okhttp3.WebSocketListener

/**
 * Cliente WebSocket customizado para o protocolo SignalR (Hub do ASP.NET Core).
 * Gerencia o handshake inicial, heartbeats (type: 6) e roteamento de invocao (type: 1).
 */
class RealtimeWebSocketClient(
    private val okHttpClient: OkHttpClient,
    private val gson: Gson,
    private val baseUrl: String
) {
    private var webSocket: WebSocket? = null
    private val coroutineScope = CoroutineScope(Dispatchers.IO)
    private val RECORD_SEPARATOR = "\u001e"

    private val _drainStatusFlow = MutableSharedFlow<DrainStatusDTO>(
        replay = 1,
        extraBufferCapacity = 64
    )
    val drainStatusFlow: SharedFlow<DrainStatusDTO> = _drainStatusFlow.asSharedFlow()

    private val _connectionErrorFlow = MutableSharedFlow<String?>()
    val connectionErrorFlow: SharedFlow<String?> = _connectionErrorFlow.asSharedFlow()

    fun connect(token: String?) {
        if (webSocket != null) {
            Log.d("RealtimeWebSocketClient", "Conexão WebSocket já está ativa. Ignorando novo pedido.")
            return
        }

        try {
            val requestBuilder = Request.Builder()
                .url(baseUrl)
                .addHeader("X-App-Id", "bueiro_inteligente_mobile_key")
            if (!token.isNullOrEmpty()) {
                requestBuilder.addHeader("Authorization", "Bearer $token")
            }

            Log.i("RealtimeWebSocketClient", "Iniciando conexao SignalR WebSocket: $baseUrl")
            webSocket = okHttpClient.newWebSocket(requestBuilder.build(), DrainWebSocketListener())
        } catch (e: Exception) {
            Log.e("RealtimeWebSocketClient", "Erro ao tentar conectar ao WebSocket", e)
        }
    }

    fun disconnect() {
        try {
            Log.d("RealtimeWebSocketClient", "Encerrando conexao WebSocket (1000/Normal)")
            webSocket?.close(1000, "App closed")
            webSocket = null
        } catch (e: Exception) {
            Log.e("RealtimeWebSocketClient", "Erro ao fechar WebSocket", e)
        }
    }

    /**
     * Invocao do mtodo JoinDrain no Hub do SignalR.
     */
    fun joinDrain(bueiroId: String) {
        val message = SignalRInvocation(
            type = 1,
            target = "JoinDrain",
            arguments = listOf(bueiroId)
        )
        sendMessage(message)
    }

    /**
     * Invocao do mtodo LeaveDrain no Hub do SignalR.
     */
    fun leaveDrain(bueiroId: String) {
        val message = SignalRInvocation(
            type = 1,
            target = "LeaveDrain",
            arguments = listOf(bueiroId)
        )
        sendMessage(message)
    }

    private fun sendMessage(message: Any) {
        val json = gson.toJson(message) + RECORD_SEPARATOR
        Log.d("RealtimeWebSocketClient", "Enviando mensagem SignalR: $json")
        webSocket?.send(json)
    }

    private inner class DrainWebSocketListener : WebSocketListener() {
        override fun onOpen(webSocket: WebSocket, response: Response) {
            Log.i("DrainWebSocketListener", "Conexao aberta. Realizando handshake SignalR...")
            
            // Handshake obrigatorio do SignalR para negociar o protocolo JSON
            val handshake = "{\"protocol\":\"json\",\"version\":1}$RECORD_SEPARATOR"
            webSocket.send(handshake)

            // Heartbeat manual para evitar idle timeout em conexes via ngrok/tneis
            coroutineScope.launch {
                while (isActive) {
                    delay(15000)
                    try {
                        val ping = "{\"type\":6}$RECORD_SEPARATOR"
                        webSocket.send(ping)
                    } catch (e: Exception) {
                        Log.e("RealtimeWebSocketClient", "Erro ao enviar SignalR Heartbeat", e)
                        break
                    }
                }
            }

            coroutineScope.launch {
                try {
                    _connectionErrorFlow.emit(null)
                } catch (e: Exception) {
                    Log.e("DrainWebSocketListener", "Erro ao limpar fluxo de erro", e)
                }
            }
        }

        override fun onMessage(webSocket: WebSocket, text: String) {
            // O SignalR pode agrupar mensagens terminadas por \u001e
            val messages = text.split(RECORD_SEPARATOR)
            for (rawMessage in messages) {
                if (rawMessage.isBlank()) continue
                
                try {
                    val signalRMessage = gson.fromJson(rawMessage, SignalRMessage::class.java)
                    
                    when (signalRMessage.type) {
                        1 -> handleInvocation(signalRMessage)
                        6 -> Log.d("DrainWebSocketListener", "Heartbeat (Type 6) recebido do Servidor")
                        else -> { /* Outros tipos ignorados: handshake ack (vazio), completion, etc */ }
                    }
                } catch (e: Exception) {
                    Log.w("DrainWebSocketListener", "Falha ao processar frame SignalR: $rawMessage", e)
                }
            }
        }

        private fun handleInvocation(message: SignalRMessage) {
            // Mapeia o evento de broadcast do Hub para o fluxo do aplicativo
            if (message.target == "BUEIRO_STATUS_MUDOU" && !message.arguments.isNullOrEmpty()) {
                try {
                    Log.i("DrainWebSocketListener", "Broadcast 'BUEIRO_STATUS_MUDOU' capturado. Fazendo parsing...")
                    // O SignalR empacota argumentos em um array JSON. 
                    // O parsing agora utiliza o JsonElement diretamente para garantir o mapeamento do DTO
                    val status = gson.fromJson(message.arguments[0], DrainStatusDTO::class.java)
                    
                    if (status != null) {
                        coroutineScope.launch {
                            _drainStatusFlow.emit(status)
                        }
                    }
                } catch (e: Exception) {
                    Log.e("DrainWebSocketListener", "Falha catastrófica no parse do DrainStatusDTO vindo do SignalR", e)
                }
            }
        }

        override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
            Log.e("DrainWebSocketListener", "Queda de conexao WebSocket detectada. Code: ${response?.code}", t)
            this@RealtimeWebSocketClient.webSocket = null
            coroutineScope.launch {
                try {
                    _connectionErrorFlow.emit("Falha na conexao de tempo real. Tentando reconectar...")
                } catch (e: Exception) {
                    Log.e("DrainWebSocketListener", "Erro ao notificar erro de conexao", e)
                }
            }
        }

        override fun onClosed(webSocket: WebSocket, code: Int, reason: String) {
            Log.i("DrainWebSocketListener", "Conexão WebSocket fechada: $code / $reason")
            this@RealtimeWebSocketClient.webSocket = null
        }
    }

    // Estrutura base de mensagens do SignalR
    private data class SignalRMessage(
        @SerializedName("type") val type: Int,
        @SerializedName("target") val target: String? = null,
        @SerializedName("arguments") val arguments: List<JsonElement>? = null
    )

    // Estrutura para envio de invocaes para o Hub (Join/Leave)
    private data class SignalRInvocation(
        @SerializedName("type") val type: Int,
        @SerializedName("target") val target: String,
        @SerializedName("arguments") val arguments: List<Any>
    )
}
