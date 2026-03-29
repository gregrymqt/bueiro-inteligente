package br.edu.fatecpg.core.network

import android.util.Log
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(private val tokenManager: TokenManager) : Interceptor {   
    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        try {
            // Ignora a injeção do token caso a rota seja de login/autenticação 
            if (originalRequest.url.encodedPath.contains("/login") || originalRequest.url.encodedPath.contains("/token")) {
                return chain.proceed(originalRequest)
            }

            val token = tokenManager.getToken()

            val requestBuilder = originalRequest.newBuilder()
            if (!token.isNullOrEmpty()) {
                requestBuilder.header("Authorization", "Bearer $token")
            }

            return chain.proceed(requestBuilder.build())
        } catch (e: Exception) {
            Log.e("AuthInterceptor", "Erro ao interceptar a requisição: $originalRequest", e)
            throw e
        }
    }
}
