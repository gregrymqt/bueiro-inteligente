package br.edu.fatecpg.core.network

import android.util.Log
import okhttp3.Authenticator
import okhttp3.Request
import okhttp3.Response
import okhttp3.Route

class TokenAuthenticator(private val tokenManager: TokenManager) : Authenticator {
    override fun authenticate(route: Route?, response: Response): Request? {    
        try {
            // Ignora caso a rota seja de autenticação para não limpar indevidamente ao tentar logar e falhar
            if (response.request.url.encodedPath.contains("/login") || response.request.url.encodedPath.contains("/token")) {
                return null
            }

            Log.w("TokenAuthenticator", "Token invalido ou expirado. Limpando token e deslogando usuario.")
            // Limpa o token forçando o fluxo reativo de navegação a expulsar o usuário
            tokenManager.clearToken()

            // Retornamos nulo pois neste fluxo não prevemos o Request novo (refresh token seria aqui)
            return null
        } catch (e: Exception) {
            Log.e("TokenAuthenticator", "Erro durante a re-autenticação: ${e.message}", e)
            return null
        }
    }
}
