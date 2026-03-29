package br.edu.fatecpg.feature.profile.repository

import android.util.Log
import br.edu.fatecpg.feature.profile.dto.UserDTO
import br.edu.fatecpg.feature.profile.services.ProfileService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.IOException

class ProfileRepository(private val service: ProfileService) {

    suspend fun fetchUserProfile(): Result<UserDTO> {
        return withContext(Dispatchers.IO) {
            try {
                Log.d("ProfileRepository", "Buscando o perfil do usuario na API")
                val response = service.getUserProfile()
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null) {
                        Log.i("ProfileRepository", "Perfil de usuario recarregado com exito")
                        Result.success(body)
                    } else {
                        Log.w("ProfileRepository", "Corpo da resposta HTTP do perfil com formato inesperado nulo.")
                        Result.failure(Exception("Corpo da resposta nulo"))     
                    }
                } else {
                    Log.w("ProfileRepository", "API rejeitou acesso as infos de perfil: ${response.code()}")
                    Result.failure(Exception(response.message() ?: "Erro ao carregar perfil"))
                }
            } catch (e: IOException) {
                Log.e("ProfileRepository", "Erro de rede io ou timeou no fetch de perfil", e)
                Result.failure(Exception("Falha de rede ao tentar carregar o perfil.", e))
            } catch (e: Exception) {
                Log.e("ProfileRepository", "Excecao desconhecida ao fetchar perfil do backend", e)
                Result.failure(e)
            }
        }
    }
}
