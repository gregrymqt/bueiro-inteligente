package br.edu.fatecpg.core.network

import android.content.Context
import android.content.SharedPreferences
import android.util.Log
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

class TokenManager(context: Context) {
    private val prefs: SharedPreferences = context.getSharedPreferences("bueiro_auth_prefs", Context.MODE_PRIVATE)

    private val _isLoggedIn = MutableStateFlow(!getToken().isNullOrEmpty())     
    val isLoggedIn: StateFlow<Boolean> = _isLoggedIn.asStateFlow()

    companion object {
        private const val KEY_JWT_TOKEN = "jwt_token"
    }

    fun saveToken(token: String) {
        try {
            prefs.edit().putString(KEY_JWT_TOKEN, token).apply()
            _isLoggedIn.value = true
            Log.d("TokenManager", "Token salvo com sucesso")
        } catch (e: Exception) {
            Log.e("TokenManager", "Erro ao salvar token", e)
        }
    }

    fun getToken(): String? {
        return try {
            prefs.getString(KEY_JWT_TOKEN, null)
        } catch (e: Exception) {
            Log.e("TokenManager", "Erro ao ler token", e)
            null
        }
    }

    fun clearToken() {
        try {
            prefs.edit().remove(KEY_JWT_TOKEN).apply()
            _isLoggedIn.value = false
            Log.d("TokenManager", "Token limpo com sucesso")
        } catch (e: Exception) {
            Log.e("TokenManager", "Erro ao limpar token", e)
        }
    }
}
