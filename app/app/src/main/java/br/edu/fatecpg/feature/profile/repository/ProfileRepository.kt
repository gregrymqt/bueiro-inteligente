package br.edu.fatecpg.feature.profile.repository

import br.edu.fatecpg.feature.profile.dto.UserDTO
import br.edu.fatecpg.feature.profile.services.ProfileService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class ProfileRepository(private val service: ProfileService) {

    suspend fun fetchUserProfile(): Result<UserDTO> {
        return withContext(Dispatchers.IO) {
            try {
                val response = service.getUserProfile()
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null) {
                        Result.success(body)
                    } else {
                        Result.failure(Exception("Corpo da resposta nulo"))
                    }
                } else {
                    Result.failure(Exception(response.message() ?: "Erro ao carregar perfil"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }
    }
}
