package br.edu.fatecpg.feature.profile.services

import android.util.Log
import br.edu.fatecpg.core.network.ApiClient
import br.edu.fatecpg.feature.profile.dto.UserDTO
import retrofit2.Response
import retrofit2.http.GET

interface ProfileService {

    @GET("users/me")
    suspend fun getUserProfile(): Response<UserDTO>

    companion object {
        fun create(): ProfileService {
            return try {
                Log.d("ProfileService", "Criando instância do ProfileService via ApiClient")
                ApiClient.createService(ProfileService::class.java)
            } catch (e: Exception) {
                Log.e("ProfileService", "Erro ao criar instância de ProfileService", e)
                throw e
            }
        }
    }
}
