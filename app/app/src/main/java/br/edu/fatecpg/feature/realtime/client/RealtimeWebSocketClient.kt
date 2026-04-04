package br.edu.fatecpg.feature.realtime.client

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import com.google.gson.Gson
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

class RealtimeWebSocketClient(
    private val okHttpClient: OkHttpClient,
    private val gson: Gson,
    private val baseUrl: String
) {
    private var webSocket: WebSocket? = null
    private val coroutineScope = CoroutineScope(Dispatchers.IO)

    private val _drainStatusFlow = MutableSharedFlow<DrainStatusDTO>()
    val drainStatusFlow: SharedFlow<DrainStatusDTO> = _drainStatusFlow.asSharedFlow()

    private val _connectionErrorFlow = MutableSharedFlow<String?>()
    val connectionErrorFlow: SharedFlow<String?> = _connectionErrorFlow.asSharedFlow()

    fun connect(token: String?) {
        try {
            val requestBuilder = Request.Builder().url(baseUrl)

            if (!token.isNullOrEmpty()) {
                requestBuilder.addHeader("Authorization", "Bearer $token")
            }

            Log.i("RealtimeWebSocketClient", "Construindo conexao OkHttp socket para (WSS/WS) url final: $baseUrl")
            webSocket = okHttpClient.newWebSocket(requestBuilder.build(), DrainWebSocketListener())
        } catch (e: Exception) {
            Log.e("RealtimeWebSocketClient", "Nao foi possivel estabelecer fabrica conexao com backend WebSocket na URL: $baseUrl", e)
        }
    }

    fun disconnect() {
        try {
            Log.d("RealtimeWebSocketClient", "Encerrando conexao WebSocket ativamente 1000/App closed")
            webSocket?.close(1000, "App closed")
            webSocket = null
        } catch (e: Exception) {
            Log.e("RealtimeWebSocketClient", "P�nico ao fechar fluxo nativo socket okhttp", e)
        }
    }

    private inner class DrainWebSocketListener : WebSocketListener() {
        override fun onOpen(webSocket: WebSocket, response: Response) {
            Log.i("DrainWebSocketListener", "Conex�o aberta de Realtime ativa com servidor")            
            // Inicia o Ping para o Render Free (evitar idle timeout de 30-55s)
            coroutineScope.launch {
                while (isActive) {
                    delay(15000)
                    try {
                        webSocket.send("ping")
                    } catch (e: Exception) {
                        Log.e("RealtimeWebSocketClient", "Erro ao enviar ping", e)
                        break
                    }
                }
            }
            coroutineScope.launch {
                try {
                    _connectionErrorFlow.emit(null)
                } catch (e: Exception) {
                    Log.e("DrainWebSocketListener", "Falha interna ao limpar erros antigos p�s reconexao no coroutine flow", e)
                }
            }
        }

        override fun onMessage(webSocket: WebSocket, text: String) {
            try {
                // Log.d("DrainWebSocketListener", "Frame WebSocket de mensagem chegado: (omitindo para n travar IO)")
                val message = gson.fromJson(text, RealtimeMessage::class.java)  
                if (message.eventoTipo == "BUEIRO_STATUS_MUDOU" && message.dados != null) {
                    Log.i("DrainWebSocketListener", "Frame recebido � update de bueiro! Realizando binding via GSON")
                    val status = gson.fromJson(gson.toJson(message.dados), DrainStatusDTO::class.java)
                    coroutineScope.launch {
                        try {
                            _drainStatusFlow.emit(status)
                        } catch (e: Exception) {
                            Log.e("DrainWebSocketListener", "Erro de emissao do SharedFlow de novo bueiro", e)
                        }
                    }
                }
            } catch (e: Exception) {
                Log.w("DrainWebSocketListener", "Falha ao desserializar formato da mensagem provida via WebSocket", e)
            }
        }

        override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
            Log.e("DrainWebSocketListener", "Nativo OkHttp WebSocket despencou (falha). Socket code: ${response?.code ?: "N/A"}", t)
            coroutineScope.launch {
                try {
                    _connectionErrorFlow.emit("Falha na conex�o de tempo real. Tentando novamente...")
                } catch (e: Exception) {
                    Log.e("DrainWebSocketListener", "Falha ao emitir mensagem de aviso visual para usuario na falha de WS", e)
                }
            }
        }
    }

    private data class RealtimeMessage(
        @SerializedName("evento_tipo") val eventoTipo: String,
        @SerializedName("dados") val dados: Any?
    )
}
