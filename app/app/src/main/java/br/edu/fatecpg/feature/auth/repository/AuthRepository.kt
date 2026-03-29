package br.edu.fatecpg.feature.auth.repository

import android.util.Log
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
    suspend fun login(credentials: LoginRequest): Result<Unit> {
        return withContext(Dispatchers.IO) {
            try {
                Log.d("AuthRepository", "Iniciando tentativa de login para o usuario: $credentials.email")
                val response = authService.login(credentials)
                if (response.isSuccessful) {
                    val tokenResponse = response.body()
                    if (tokenResponse != null) {
                        try {
                            tokenManager.saveToken(tokenResponse.accessToken)       
                            Log.i("AuthRepository", "Login efetuado com sucesso e token salvo localmente.")
                            Result.success(Unit)
                        } catch (e: Exception) {
                            Log.e("AuthRepository", "Erro ao salvar token de acesso no dispositivo.", e)
                            Result.failure(Exception("Erro na autenticação local."))
                        }
                    } else {
                        Log.w("AuthRepository", "Falha de Login: Resposta da rede vazia apesar do status 200.")
                        Result.failure(Exception("Resposta do servidor vazia."))
                    }
                } else {
                    Log.w("AuthRepository", "Falha de Login: Codigo HTTP ${response.code()}")
                    Result.failure(Exception("Erro na autenticação (Código: ${response.code()})."))
                }
            } catch (e: Exception) {
                Log.e("AuthRepository", "Erro crítico de rede ou processamento no login: ${e.message}", e)
                Result.failure(Exception("Falha de conexão. Verifique sua rede e tente novamente."))
            }
        }
    }

    fun isUserLoggedIn(): Boolean {
        try {
            return !tokenManager.getToken().isNullOrEmpty()
        } catch (e: Exception) {
            Log.e("AuthRepository", "Erro ao recuperar estado do usuário logado", e)
            return false
        }
    }

    suspend fun register(request: RegisterRequest): Result<UserDTO> {
        return withContext(Dispatchers.IO) {
            try {
                Log.d("AuthRepository", "Iniciando tentativa de registro para o usuario: $request.email")
                val response = authService.register(request)
                if (response.isSuccessful) {
                    val userResponse = response.body()
                    if (userResponse != null) {
                        Log.i("AuthRepository", "Registro efetuado com sucesso para o banco de dados via API.")
                        Result.success(userResponse)
                    } else {
                        Log.w("AuthRepository", "Falha de Registro: Resposta do servidor vazia.")
                        Result.failure(Exception("Resposta do servidor vazia."))
                    }
                } else {
                    Log.w("AuthRepository", "Erro de Registro na API: Cógigo ${response.code()}")
                    Result.failure(Exception("Erro no cadastro (Código: ${response.code()})."))
                }
            } catch (e: Exception) {
                Log.e("AuthRepository", "Erro crítico de rede no registro: ${e.message}", e)
                Result.failure(Exception("Falha de conexão. Verifique sua rede e tente novamente."))
            }
        }
    }

    suspend fun logout(): Result<Unit> {
        return withContext(Dispatchers.IO) {
            try {
                Log.d("AuthRepository", "Iniciando processo de logout na API.")
                // Tenta chamar o endpoint de logout na API
                authService.logout()
                Log.i("AuthRepository", "Logout feito na API com sucesso.")
            } catch (e: Exception) {
                Log.w("AuthRepository", "Falha ao fechar sessao na API (possível sem rede), limpando dados locais. Erro: ${e.message}")
                // Ignoramos erros de rede, pois o comportamento local deve prevalecer
            } finally {
                try {
                    // Sempre removemos o token localmente, garantindo o logout no dispositivo
                    tokenManager.clearToken()
                    Log.i("AuthRepository", "Token local apagado devido ao logout")
                } catch (e: Exception) {
                    Log.e("AuthRepository", "Erro ao apagar token durante logout", e)
                }
            }
            Result.success(Unit)
        }
    }
}
