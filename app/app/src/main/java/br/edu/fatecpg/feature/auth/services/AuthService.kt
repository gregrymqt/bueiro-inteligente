package br.edu.fatecpg.feature.auth.services

import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.feature.auth.dto.LoginRequest
import br.edu.fatecpg.feature.auth.dto.RegisterRequest
import br.edu.fatecpg.feature.auth.dto.TokenResponse
import br.edu.fatecpg.feature.auth.dto.UserDTO
import okhttp3.ResponseBody
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import android.util.Log

interface AuthService {

    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): Response<TokenResponse>     

    @POST("auth/register")
    suspend fun register(@Body request: RegisterRequest): Response<UserDTO>     

    @POST("auth/logout")
    suspend fun logout(): Response<ResponseBody>

    @GET("auth/users/me")
    suspend fun getCurrentUser(): Response<UserDTO>

    companion object {
        fun create(): AuthService {
            return try {
                Log.d("AuthService", "Criando instância do AuthService via ApiClient")
                ApiClient.createService(AuthService::class.java)
            } catch (e: Exception) {
                Log.e("AuthService", "Erro ao criar instância do AuthService", e)
                throw e
            }
        }
    }
}
