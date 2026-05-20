package br.edu.fatecpg.feature.auth.repository

import android.util.Log
import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.dto.LoginRequest
import br.edu.fatecpg.feature.auth.dto.RegisterRequest
import br.edu.fatecpg.feature.auth.dto.UserDTO
import br.edu.fatecpg.feature.auth.services.AuthService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class AuthRepository(
    private val authService: AuthService,
    private val tokenManager: TokenManager,
    private val localCacheService: LocalCacheService
) {

    private companion object {
        const val KEY_USER_PROFILE = "cached_user_profile"
        const val CACHE_EXPIRY = 10 * 60 * 1000L // 10 minutos
        const val TAG = "AuthRepository"
    }

    suspend fun login(request: LoginRequest): Result<Boolean> {
        return withContext(Dispatchers.IO) {
            val responseResult = authService.login(request)
            if (responseResult.isSuccess) {
                val tokenResponse = responseResult.getOrThrow()
                tokenManager.saveToken(tokenResponse.accessToken)
                Result.success(true)
            } else {
                Result.failure(responseResult.exceptionOrNull() ?: Exception("Login failed"))
            }
        }
    }

    suspend fun register(request: RegisterRequest): Result<UserDTO> {
        return withContext(Dispatchers.IO) {
            val responseResult = authService.register(request)
            if (responseResult.isSuccess) {
                Result.success(responseResult.getOrThrow())
            } else {
                Result.failure(responseResult.exceptionOrNull() ?: Exception("Registration failed"))
            }
        }
    }

    suspend fun getCurrentUser(): Result<UserDTO> {
        return withContext(Dispatchers.IO) {
            runCatching {
                localCacheService.getOrSet(KEY_USER_PROFILE, CACHE_EXPIRY) {
                    authService.getCurrentUser().getOrThrow()
                }
            }.onFailure {
                Log.w(TAG, "Falha ao obter usuário (Rede + Cache): ${it.message}")
            }
        }
    }

    suspend fun logout() {
        tokenManager.clearToken()
        localCacheService.remove(KEY_USER_PROFILE)
        Log.d(TAG, "Sessão encerrada e cache de perfil removido")
    }
}
