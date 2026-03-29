package br.edu.fatecpg.feature.auth.repository

import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.dto.LoginRequest
import br.edu.fatecpg.feature.auth.services.AuthService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class AuthRepository(
    private val authService: AuthService,
    private val tokenManager: TokenManager
) {
    suspend fun login(credentials: LoginRequest): Result<Unit> {
        return withContext(Dispatchers.IO) {
            try {
                val response = authService.login(credentials)
                if (response.isSuccessful) {
                    val tokenResponse = response.body()
                    if (tokenResponse != null) {
                        tokenManager.saveToken(tokenResponse.accessToken)
                        Result.success(Unit)
                    } else {
                        Result.failure(Exception("Resposta do servidor vazia."))
                    }
                } else {
                    Result.failure(Exception("Erro na autenticação (Código: ${response.code()})."))
                }
            } catch (e: Exception) {
                Result.failure(Exception("Falha de conexão. Verifique sua rede e tente novamente."))
            }
        }
    }

    fun isUserLoggedIn(): Boolean {
        return !tokenManager.getToken().isNullOrEmpty()
    }

    suspend fun logout(): Result<Unit> {
        return withContext(Dispatchers.IO) {
            try {
                // Tenta chamar o endpoint de logout na API
                authService.logout()
            } catch (e: Exception) {
                // Ignoramos erros de rede, pois o comportamento local deve prevalecer
            } finally {
                // Sempre removemos o token localmente, garantindo o logout no dispositivo
                tokenManager.clearToken()
            }
            Result.success(Unit)
        }
    }
}