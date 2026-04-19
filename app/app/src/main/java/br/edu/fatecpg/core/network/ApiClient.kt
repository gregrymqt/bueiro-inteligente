package br.edu.fatecpg.core.network

import android.util.Log
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {
    lateinit var httpClient: OkHttpClient
    private var retrofit: Retrofit? = null    

    /**
     * Inicializa a inst�ncia Singleton do Retrofit.
     * Deve ser chamado idealmente no Application ou na Activity inicial.       
     */
    fun init(tokenManager: TokenManager, baseUrl: String) {
        if (retrofit == null) {
            try {
                Log.i("ApiClient", "Inicializando ApiClient com base url: $baseUrl")
                
                val loggingInterceptor = HttpLoggingInterceptor().apply {
                    level = HttpLoggingInterceptor.Level.BODY
                }

                val authInterceptor = AuthInterceptor(tokenManager)
                val appIdInterceptor = AppIdInterceptor()

                val tokenAuthenticator = TokenAuthenticator(tokenManager)
                val rateLimitInterceptor = RateLimitInterceptor()

                val okHttpClient = OkHttpClient.Builder()
                    .addInterceptor(authInterceptor)
                    .addInterceptor(appIdInterceptor)
                    .addInterceptor(rateLimitInterceptor)
                    .authenticator(tokenAuthenticator)
                    .addInterceptor(loggingInterceptor) // Adicionado para debug em nvel BODY
                    .connectTimeout(60, TimeUnit.SECONDS)
                    .readTimeout(60, TimeUnit.SECONDS)
                    .pingInterval(15, TimeUnit.SECONDS) // Ping de WebSocket nativo para o Render Free
                    .build()
                
                this.httpClient = okHttpClient

                retrofit = Retrofit.Builder()
                    .baseUrl(baseUrl)
                    .client(okHttpClient)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build()
                
                Log.i("ApiClient", "ApiClient inicializado com sucesso.")
            } catch (e: Exception) {
                Log.e("ApiClient", "Erro critico ao inicializar ApiClient: ${e.message}", e)
                throw e
            }
        }
    }

    /**
     * Cria e retorna uma interface de servi�o usando a inst�ncia atual do Retrofit.
     */
    fun <T> createService(serviceClass: Class<T>): T {
        try {
            val currentRetrofit = retrofit ?: throw IllegalStateException("ApiClient not initialized. Call init() first.")
            return currentRetrofit.create(serviceClass)
        } catch (e: Exception) {
            Log.e("ApiClient", "Erro ao criar servico ${serviceClass.simpleName}: ${e.message}", e)
            throw e
        }
    }
}
