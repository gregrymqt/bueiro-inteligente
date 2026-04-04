package br.edu.fatecpg.core.network

import okhttp3.Interceptor
import okhttp3.Response
import java.io.IOException

class RateLimitInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val response = chain.proceed(request)

        if (response.code == 429) {
            // Emissão de evento para a UI ou logs
            // TODO: Integrar com um evento global de estado (Flow/SharedFlow) para notificar a UI de "Muitas tentativas".
            throw RateLimitException("Muitas requisições. Por favor, aguarde um momento antes de tentar novamente.")
        }

        return response
    }
}

class RateLimitException(message: String) : IOException(message)
