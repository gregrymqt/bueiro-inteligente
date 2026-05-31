package br.edu.fatecpg.feature.auth.viewmodel

import android.util.Log
import br.edu.fatecpg.feature.auth.dto.RegisterRequest
import br.edu.fatecpg.feature.auth.dto.UserDTO
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import io.mockk.clearAllMocks
import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.confirmVerified
import io.mockk.mockk
import io.mockk.mockkStatic
import io.mockk.unmockkStatic
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.Job
import kotlinx.coroutines.CoroutineStart
import kotlinx.coroutines.launch
import kotlinx.coroutines.test.StandardTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.setMain
import kotlinx.coroutines.yield
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class RegisterViewModelTest {

    private val testDispatcher = StandardTestDispatcher()
    private val repository = mockk<AuthRepository>()

    private lateinit var viewModel: RegisterViewModel

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
        mockkStatic(Log::class)
        everyLogReturnsZero()
        viewModel = RegisterViewModel(repository)
    }

    @After
    fun tearDown() {
        clearAllMocks()
        unmockkStatic(Log::class)
        Dispatchers.resetMain()
    }

    @Test
    fun performRegister_comQualquerCampoEmBranco_deveRetornarError() {
        // Arrange
        val email = "usuario@teste.com"
        val password = "Senha123!"
        val fullName = ""

        // Act
        val emissions = mutableListOf<RegisterUiState>()
        val collectorJob = collectUiState(emissions)

        try {
            viewModel.performRegister(email, password, fullName)
            testDispatcher.scheduler.advanceUntilIdle()

            // Assert
            assertEquals(
                listOf(
                    RegisterUiState.Idle,
                    RegisterUiState.Error("Todos os campos devem ser preenchidos.")
                ),
                emissions
            )
            coVerify(exactly = 0) { repository.register(any()) }
            confirmVerified(repository)
        } finally {
            collectorJob.cancel()
        }
    }

    @Test
    fun performRegister_comEmailInvalido_deveRetornarError() {
        // Arrange
        val email = "usuario@teste"
        val password = "Senha123!"
        val fullName = "Fulano da Silva"

        // Act
        val emissions = mutableListOf<RegisterUiState>()
        val collectorJob = collectUiState(emissions)

        try {
            viewModel.performRegister(email, password, fullName)
            testDispatcher.scheduler.advanceUntilIdle()

            // Assert
            assertEquals(
                listOf(
                    RegisterUiState.Idle,
                    RegisterUiState.Error("Insira um e-mail válido.")
                ),
                emissions
            )
            coVerify(exactly = 0) { repository.register(any()) }
            confirmVerified(repository)
        } finally {
            collectorJob.cancel()
        }
    }

    @Test
    fun performRegister_comSenhaFraca_deveRetornarError() {
        // Arrange
        val email = "usuario@teste.com"
        val password = "senha123"
        val fullName = "Fulano da Silva"

        // Act
        val emissions = mutableListOf<RegisterUiState>()
        val collectorJob = collectUiState(emissions)

        try {
            viewModel.performRegister(email, password, fullName)
            testDispatcher.scheduler.advanceUntilIdle()

            // Assert
            assertEquals(
                listOf(
                    RegisterUiState.Idle,
                    RegisterUiState.Error("A senha deve ter 8+ caracteres, incluindo maiúsculas, números e símbolos.")
                ),
                emissions
            )
            coVerify(exactly = 0) { repository.register(any()) }
            confirmVerified(repository)
        } finally {
            collectorJob.cancel()
        }
    }

    @Test
    fun performRegister_comDadosValidosESucessoNoRepositorio_deveTransicionarParaSuccess() {
        // Arrange
        val emailInput = "  usuario@teste.com  "
        val trimmedEmail = emailInput.trim()
        val password = "Senha123!"
        val fullName = "Fulano da Silva"
        val expectedRequest = RegisterRequest(trimmedEmail, password, fullName)
        val expectedUser = UserDTO(email = trimmedEmail, fullName = fullName, roles = listOf("USER"))
        coEvery { repository.register(expectedRequest) } coAnswers {
            yield()
            Result.success(expectedUser)
        }

        // Act
        val emissions = mutableListOf<RegisterUiState>()
        val collectorJob = collectUiState(emissions)

        try {
            viewModel.performRegister(emailInput, password, fullName)
            testDispatcher.scheduler.advanceUntilIdle()

            // Assert
            assertEquals(
                listOf(
                    RegisterUiState.Idle,
                    RegisterUiState.Loading,
                    RegisterUiState.Success
                ),
                emissions
            )
            coVerify(exactly = 1) { repository.register(expectedRequest) }
            confirmVerified(repository)
        } finally {
            collectorJob.cancel()
        }
    }

    @Test
    fun performRegister_quandoRepositorioFalha_deveRetornarErrorComMensagemEspecifica() {
        // Arrange
        val email = "usuario@teste.com"
        val password = "Senha123!"
        val fullName = "Fulano da Silva"
        val expectedRequest = RegisterRequest(email, password, fullName)
        val serverMessage = "E-mail já cadastrado."
        coEvery { repository.register(expectedRequest) } coAnswers {
            yield()
            Result.failure(Exception(serverMessage))
        }

        // Act
        val emissions = mutableListOf<RegisterUiState>()
        val collectorJob = collectUiState(emissions)

        try {
            viewModel.performRegister(email, password, fullName)
            testDispatcher.scheduler.advanceUntilIdle()

            // Assert
            assertEquals(
                listOf(
                    RegisterUiState.Idle,
                    RegisterUiState.Loading,
                    RegisterUiState.Error(serverMessage)
                ),
                emissions
            )
            coVerify(exactly = 1) { repository.register(expectedRequest) }
            confirmVerified(repository)
        } finally {
            collectorJob.cancel()
        }
    }

    private fun collectUiState(emissions: MutableList<RegisterUiState>): Job {
        return CoroutineScope(testDispatcher).launch(start = CoroutineStart.UNDISPATCHED) {
            viewModel.uiState.collect { emissions.add(it) }
        }
    }

    private fun everyLogReturnsZero() {
        io.mockk.every { Log.d(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.i(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.w(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.e(any<String>(), any<String>()) } returns 0
        io.mockk.every { Log.e(any<String>(), any<String>(), any<Throwable>()) } returns 0
    }
}