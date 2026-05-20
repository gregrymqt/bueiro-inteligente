package br.edu.fatecpg.feature.auth.repository

import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.dto.LoginRequest
import br.edu.fatecpg.feature.auth.dto.RegisterRequest
import br.edu.fatecpg.feature.auth.dto.UserDTO
import br.edu.fatecpg.feature.auth.services.AuthService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class AuthRepository(
    private val authService: AuthService,
    private val tokenManager: TokenManager
) {

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
            val responseResult = authService.getCurrentUser()
            if (responseResult.isSuccess) {
                Result.success(responseResult.getOrThrow())
            } else {
                Result.failure(responseResult.exceptionOrNull() ?: Exception("Failed to fetch user"))
            }
        }
    }

    fun logout() {
        tokenManager.clearToken()
    }
}
