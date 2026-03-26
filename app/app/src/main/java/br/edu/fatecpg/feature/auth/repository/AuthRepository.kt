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
                    if (response.code() == 401) {
                        Result.failure(Exception("Credenciais inválidas. Verifique seu e-mail e senha."))
                    } else {
                        Result.failure(Exception("Erro na autenticação (Código: ${response.code()})."))
                    }
                }
            } catch (e: Exception) {
                Result.failure(Exception("Falha de conexão. Verifique sua rede e tente novamente."))
            }
        }
    }

    fun isUserLoggedIn(): Boolean {
        return !tokenManager.getToken().isNullOrEmpty()
    }
}