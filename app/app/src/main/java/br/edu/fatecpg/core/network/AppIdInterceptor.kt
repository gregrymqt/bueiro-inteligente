package br.edu.fatecpg.core.network

import android.util.Log
import br.edu.fatecpg.BuildConfig
import okhttp3.Interceptor
import okhttp3.Response

class AppIdInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        return try {
            val requestBuilder = originalRequest.newBuilder()
                .header("X-App-Id", BuildConfig.APP_ID_SECRET)

            chain.proceed(requestBuilder.build())
        } catch (e: Exception) {
            Log.e("AppIdInterceptor", "Erro ao injetar X-App-Id em: $originalRequest", e)
            throw e
        }
    }
}