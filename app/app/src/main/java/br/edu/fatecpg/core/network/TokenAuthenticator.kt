package br.edu.fatecpg.core.network

import okhttp3.Authenticator
import okhttp3.Request
import okhttp3.Response
import okhttp3.Route

class TokenAuthenticator(private val tokenManager: TokenManager) : Authenticator {
    override fun authenticate(route: Route?, response: Response): Request? {
        // Ignora caso a rota seja de autenticaÃ§Ã£o para nÃ£o limpar indevidamente ao tentar logar e falhar
        if (response.request.url.encodedPath.contains("/login") || response.request.url.encodedPath.contains("/token")) {
            return null
        }

        // Limpa o token forÃ§ando o fluxo reativo de navegaÃ§Ã£o a expulsar o usuÃ¡rio
        tokenManager.clearToken()
        
        // Retornamos nulo pois neste fluxo nÃ£o prevemos o Request novo (refresh token seria aqui)
        return null
    }
}
