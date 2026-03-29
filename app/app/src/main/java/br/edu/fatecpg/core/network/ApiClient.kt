package br.edu.fatecpg.core.network

import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {
    private var retrofit: Retrofit? = null

    /**
     * Inicializa a instância Singleton do Retrofit.
     * Deve ser chamado idealmente no Application ou na Activity inicial.
     */
    fun init(tokenManager: TokenManager, baseUrl: String) {
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
                .baseUrl(baseUrl)
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
