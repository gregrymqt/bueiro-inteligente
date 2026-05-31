package br.edu.fatecpg.feature.auth.viewmodel

import android.util.Log
import br.edu.fatecpg.feature.auth.dto.LoginRequest
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import io.mockk.clearAllMocks
import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.confirmVerified
import io.mockk.mockk
import io.mockk.mockkStatic
import io.mockk.unmockkStatic
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.StandardTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class LoginViewModelTest {

    private val testDispatcher = StandardTestDispatcher()
    private val repository = mockk<AuthRepository>()

    private lateinit var viewModel: LoginViewModel

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
        mockkStatic(Log::class)
        everyLogReturnsZero()
        viewModel = LoginViewModel(repository)
    }

    @After
    fun tearDown() {
        clearAllMocks()
        unmockkStatic(Log::class)
        Dispatchers.resetMain()
    }

    @Test
    fun performLogin_comCamposVazios_deveDefinirEstadoComoError() {
        // Arrange
        val expectedMessage = "Preencha todos os campos para continuar."

        // Act
        viewModel.performLogin("", "")

        // Assert
        assertEquals(LoginUiState.Error(expectedMessage), viewModel.uiState.value)
        coVerify(exactly = 0) { repository.login(any()) }
        confirmVerified(repository)
    }

    @Test
    fun performLogin_comSucessoNoRepositorio_deveTransicionarParaSuccess() {
        // Arrange
        val email = "usuario@teste.com"
        val password = "Senha123!"
        coEvery { repository.login(LoginRequest(email, password)) } returns Result.success(true)

        // Act
        viewModel.performLogin(email, password)

        // Assert
        assertEquals(LoginUiState.Loading, viewModel.uiState.value)
        testDispatcher.scheduler.advanceUntilIdle()
        assertEquals(LoginUiState.Success, viewModel.uiState.value)
        coVerify(exactly = 1) { repository.login(LoginRequest(email, password)) }
        confirmVerified(repository)
    }

    @Test
    fun performLogin_comFalhaNoRepositorio_deveTransicionarParaErrorComMensagemDoServidor() {
        // Arrange
        val email = "usuario@teste.com"
        val password = "Senha123!"
        val serverMessage = "Falha ao autenticar no servidor."
        coEvery { repository.login(LoginRequest(email, password)) } returns Result.failure(Exception(serverMessage))

        // Act
        viewModel.performLogin(email, password)

        // Assert
        assertEquals(LoginUiState.Loading, viewModel.uiState.value)
        testDispatcher.scheduler.advanceUntilIdle()
        assertEquals(LoginUiState.Error(serverMessage), viewModel.uiState.value)
        coVerify(exactly = 1) { repository.login(LoginRequest(email, password)) }
        confirmVerified(repository)
    }

    @Test
    fun performLogin_comExcecaoNoRepositorio_deveTratarErroEDefinirErrorInesperado() {
        // Arrange
        val email = "usuario@teste.com"
        val password = "Senha123!"
        coEvery { repository.login(LoginRequest(email, password)) } throws Exception("Erro de infraestrutura")

        // Act
        viewModel.performLogin(email, password)

        // Assert
        assertEquals(LoginUiState.Loading, viewModel.uiState.value)
        testDispatcher.scheduler.advanceUntilIdle()
        assertEquals(LoginUiState.Error("Erro inesperado durante a tentativa de login."), viewModel.uiState.value)
        coVerify(exactly = 1) { repository.login(LoginRequest(email, password)) }
        confirmVerified(repository)
    }

    @Test
    fun resetState_quandoChamado_deveVoltarOEstadoParaIdle() {
        // Arrange
        val email = "usuario@teste.com"
        val password = "Senha123!"
        coEvery { repository.login(LoginRequest(email, password)) } returns Result.success(true)

        viewModel.performLogin(email, password)
        testDispatcher.scheduler.advanceUntilIdle()
        assertEquals(LoginUiState.Success, viewModel.uiState.value)

        // Act
        viewModel.resetState()

        // Assert
        assertEquals(LoginUiState.Idle, viewModel.uiState.value)
        coVerify(exactly = 1) { repository.login(LoginRequest(email, password)) }
        confirmVerified(repository)
    }

    private fun everyLogReturnsZero() {
        io.mockk.every { Log.d(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.i(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.w(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.e(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.e(any<String>(), any<String>(), any<Throwable>()) } returns 0
    }
}