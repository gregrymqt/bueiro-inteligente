package br.edu.fatecpg.core.http

import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {
    // Altere para o IP apropriado. 
    // Em emuladores Android, 10.0.2.2 mapeia para o localhost (127.0.0.1) da máquina hospedeira onde a API FastAPI estará rodando.
    private const val BASE_URL = "http://10.0.2.2:8000/"

    fun build(tokenService: TokenService): Retrofit {
        val authInterceptor = AuthInterceptor(tokenService)

        val client = OkHttpClient.Builder()
            .addInterceptor(authInterceptor)
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()

        return Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }
}
