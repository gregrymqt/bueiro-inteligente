package br.edu.fatecpg.core.http

import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(private val tokenService: TokenService) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        
        // Evita enviar o token nas rotas de login
        if (request.url.encodedPath.contains("/login") || request.url.encodedPath.contains("/token")) {
            return chain.proceed(request)
        }

        val token = tokenService.getToken()
        return if (!token.isNullOrEmpty()) {
            val newRequest = request.newBuilder()
                .header("Authorization", "Bearer $token")
                .build()
            chain.proceed(newRequest)
        } else {
            chain.proceed(request)
        }
    }
}
