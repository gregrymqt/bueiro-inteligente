package br.edu.fatecpg.feature.auth.repository

import android.util.Log
import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.auth.dto.LoginRequest
import br.edu.fatecpg.feature.auth.dto.RegisterRequest
import br.edu.fatecpg.feature.auth.dto.TokenResponse
import br.edu.fatecpg.feature.auth.dto.UserDTO
import br.edu.fatecpg.feature.auth.services.AuthService
import io.mockk.clearAllMocks
import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.confirmVerified
import io.mockk.every
import io.mockk.mockk
import io.mockk.mockkStatic
import io.mockk.unmockkStatic
import io.mockk.verify
import kotlinx.coroutines.test.runTest
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertSame
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class AuthRepositoryTest {

    private val authService = mockk<AuthService>()
    private val tokenManager = mockk<TokenManager>(relaxed = true)
    private val localCacheService = mockk<LocalCacheService>(relaxed = true)

    private val repository = AuthRepository(authService, tokenManager, localCacheService)

    @Before
    fun setUp() {
        mockkStatic(Log::class)
        stubAndroidLog()
    }

    @After
    fun tearDown() {
        clearAllMocks()
        unmockkStatic(Log::class)
    }

    @Test
    fun login_comSucesso_deveSalvarTokenNoTokenManagerERetornarSuccessTrue() = runTest {
        // Arrange
        val request = LoginRequest(
            email = "usuario@teste.com",
            password = "Senha123!"
        )
        val tokenResponse = TokenResponse(
            accessToken = "token-123",
            tokenType = "Bearer"
        )
        coEvery { authService.login(request) } returns Result.success(tokenResponse)

        // Act
        val result = repository.login(request)

        // Assert
        assertTrue(result.isSuccess)
        assertEquals(true, result.getOrNull())
        verifyTokenSaved("token-123")
        coVerify(exactly = 1) { authService.login(request) }
        confirmVerified(authService, tokenManager, localCacheService)
    }

    @Test
    fun login_comFalhaNaRede_deveRetornarFailureComException() = runTest {
        // Arrange
        val request = LoginRequest(
            email = "usuario@teste.com",
            password = "Senha123!"
        )
        val exception = RuntimeException("401 Unauthorized")
        coEvery { authService.login(request) } returns Result.failure(exception)

        // Act
        val result = repository.login(request)

        // Assert
        assertTrue(result.isFailure)
        assertSame(exception, result.exceptionOrNull())
        coVerify(exactly = 1) { authService.login(request) }
        verify(exactly = 0) { tokenManager.saveToken(any()) }
        confirmVerified(authService, tokenManager, localCacheService)
    }

    @Test
    fun register_comSucesso_deveRetornarResultSuccessComUserDTO() = runTest {
        // Arrange
        val request = RegisterRequest(
            email = "usuario@teste.com",
            password = "Senha123!",
            fullName = "Fulano da Silva"
        )
        val expectedUser = UserDTO(
            email = request.email,
            fullName = request.fullName,
            roles = listOf("USER")
        )
        coEvery { authService.register(request) } returns Result.success(expectedUser)

        // Act
        val result = repository.register(request)

        // Assert
        assertTrue(result.isSuccess)
        assertEquals(expectedUser, result.getOrNull())
        coVerify(exactly = 1) { authService.register(request) }
        confirmVerified(authService, tokenManager, localCacheService)
    }

    @Test
    fun getCurrentUser_deveChamarLocalCacheServiceComChaveETempoCorretos() = runTest {
        // Arrange
        val expectedUser = UserDTO(
            email = "usuario@teste.com",
            fullName = "Fulano da Silva",
            roles = listOf("USER")
        )
        coEvery { localCacheService.getOrSet<UserDTO>(CACHE_USER_PROFILE, CACHE_EXPIRY, any()) } returns expectedUser

        // Act
        val result = repository.getCurrentUser()

        // Assert
        assertTrue(result.isSuccess)
        assertEquals(expectedUser, result.getOrNull())
        coVerify(exactly = 1) { localCacheService.getOrSet<UserDTO>(CACHE_USER_PROFILE, CACHE_EXPIRY, any()) }
        confirmVerified(authService, tokenManager, localCacheService)
    }

    @Test
    fun logout_deveChamarServicoDeRede_limparTokenManager_E_removerUserDoCache() = runTest {
        // Arrange
        coEvery { authService.logout() } returns Result.success(Unit)

        // Act
        repository.logout()

        // Assert
        coVerify(exactly = 1) { authService.logout() }
        verify(exactly = 1) { tokenManager.clearToken() }
        coVerify(exactly = 1) { localCacheService.remove(CACHE_USER_PROFILE) }
        confirmVerified(authService, tokenManager, localCacheService)
    }

    private fun stubAndroidLog() {
        every { Log.d(any<String>(), any<String>()) } returns 0
        every { Log.i(any<String>(), any<String>()) } returns 0
        every { Log.w(any<String>(), any<String>()) } returns 0
        every { Log.w(any<String>(), any<String>(), any<Throwable>()) } returns 0
        every { Log.e(any<String>(), any<String>()) } returns 0
        every { Log.e(any<String>(), any<String>(), any<Throwable>()) } returns 0
    }

    private fun verifyTokenSaved(token: String) {
        verify(exactly = 1) { tokenManager.saveToken(token) }
    }

    private companion object {
        const val CACHE_USER_PROFILE = "cached_user_profile"
        const val CACHE_EXPIRY = 10 * 60 * 1000L
    }
}