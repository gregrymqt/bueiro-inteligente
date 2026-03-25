package br.edu.fatecpg.core.network

import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {
    private const val BASE_URL = "http://10.0.2.2:8000/"
    private var retrofit: Retrofit? = null

    /**
     * Inicializa a instância Singleton do Retrofit.
     * Deve ser chamado idealmente no Application ou na Activity inicial.
     */
    fun init(tokenManager: TokenManager) {
        if (retrofit == null) {
            val loggingInterceptor = HttpLoggingInterceptor().apply {
                level = HttpLoggingInterceptor.Level.BODY
            }

            val authInterceptor = AuthInterceptor(tokenManager)

            val okHttpClient = OkHttpClient.Builder()
                .addInterceptor(authInterceptor)
                .addInterceptor(loggingInterceptor) // Adicionado para debug em nível BODY
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .build()

            retrofit = Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(okHttpClient)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
        }
    }

    /**
     * Método genérico para instanciar serviços do Retrofit.
     */
    fun <T> createService(serviceClass: Class<T>): T {
        val currentRetrofit = retrofit ?: throw IllegalStateException(
            "ApiClient não foi inicializado. Chame ApiClient.init(tokenManager) primeiro."
        )
        return currentRetrofit.create(serviceClass)
    }
}
