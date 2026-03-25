package br.edu.fatecpg.feature.realtime.services

import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import com.google.gson.Gson
import com.google.gson.JsonObject
import kotlinx.coroutines.channels.BufferOverflow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.Response
import okhttp3.WebSocket
import okhttp3.WebSocketListener
import java.util.concurrent.TimeUnit

class RealtimeService {

    private val okHttpClient = OkHttpClient.Builder()
        .readTimeout(0, TimeUnit.MILLISECONDS)
        .build()

    private val gson = Gson()
    private var webSocket: WebSocket? = null

    private val _alertas = MutableSharedFlow<DrainStatusDTO>(
        extraBufferCapacity = 10,
        onBufferOverflow = BufferOverflow.DROP_OLDEST
    )
    val alertas: SharedFlow<DrainStatusDTO> = _alertas.asSharedFlow()

    fun connect(token: String?) {
        if (webSocket != null) return // Já está conectado

        val requestBuilder = Request.Builder().url("ws://10.0.2.2:8000/realtime/ws")
        if (!token.isNullOrEmpty()) {
            requestBuilder.addHeader("Authorization", "Bearer $token")
        }

        webSocket = okHttpClient.newWebSocket(requestBuilder.build(), object : WebSocketListener() {
            override fun onMessage(webSocket: WebSocket, text: String) {
                try {
                    val jsonObject = gson.fromJson(text, JsonObject::class.java)

                    // Verificando os campos conforme a nova especificação: 'evento_tipo' e 'dados'
                    if (jsonObject.has("evento_tipo") && jsonObject.get("evento_tipo").asString == "BUEIRO_STATUS_MUDOU") {
                        val dadosJson = jsonObject.getAsJsonObject("dados")
                        val drainStatus = gson.fromJson(dadosJson, DrainStatusDTO::class.java)
                        
                        _alertas.tryEmit(drainStatus)
                    }
                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }

            override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
                super.onFailure(webSocket, t, response)
                t.printStackTrace()
                // Em cenário real, aqui seria o local ideal para lógica de reconexão automática
            }
        })
    }

    fun disconnect() {
        webSocket?.close(1000, "App moved to background")
        webSocket = null
    }
}
