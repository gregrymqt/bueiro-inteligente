package br.edu.fatecpg.feature.profile.viewmodel

import android.util.Log
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.feature.auth.dto.UserDTO
import br.edu.fatecpg.feature.auth.repository.AuthRepository
import io.mockk.clearAllMocks
import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.confirmVerified
import io.mockk.every
import io.mockk.mockk
import io.mockk.mockkStatic
import io.mockk.unmockkStatic
import io.mockk.verify
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.UnconfinedTestDispatcher
import kotlinx.coroutines.test.advanceUntilIdle
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import kotlinx.coroutines.delay
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class ProfileViewModelTest {

    private val testDispatcher = UnconfinedTestDispatcher()
    private val repository = mockk<AuthRepository>()
    private val locationHandler = mockk<LocationHandler>(relaxed = true)

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
        mockkStatic(Log::class)
        stubAndroidLog()
    }

    @After
    fun tearDown() {
        clearAllMocks()
        unmockkStatic(Log::class)
        Dispatchers.resetMain()
    }

    @Test
    fun loadProfile_comSucesso_deveDefinirUiStateComoSuccess() = runTest(testDispatcher) {
        // Arrange
        val expectedUser = createUser()
        coEvery { repository.getCurrentUser() } coAnswers {
            delay(1)
            Result.success(expectedUser)
        }
        val viewModel = createViewModel()

        // Act
        viewModel.onAction(ProfileAction.LoadProfile)

        // Assert
        assertEquals(ProfileUiState.Loading, viewModel.uiState.value)
        advanceUntilIdle()
        assertEquals(ProfileUiState.Success(expectedUser), viewModel.uiState.value)
        coVerify(exactly = 1) { repository.getCurrentUser() }
        verify(exactly = 0) { locationHandler.openWebUrl(any()) }
        confirmVerified(repository, locationHandler)
    }

    @Test
    fun loadProfile_comFalha_deveDefinirUiStateComoErrorComMensagemDoServidor() = runTest(testDispatcher) {
        // Arrange
        val expectedMessage = "Falha ao carregar o perfil do servidor."
        coEvery { repository.getCurrentUser() } coAnswers {
            delay(1)
            Result.failure(Exception(expectedMessage))
        }
        val viewModel = createViewModel()

        // Act
        viewModel.onAction(ProfileAction.LoadProfile)

        // Assert
        assertEquals(ProfileUiState.Loading, viewModel.uiState.value)
        advanceUntilIdle()
        assertEquals(ProfileUiState.Error(expectedMessage), viewModel.uiState.value)
        coVerify(exactly = 1) { repository.getCurrentUser() }
        verify(exactly = 0) { locationHandler.openWebUrl(any()) }
        confirmVerified(repository, locationHandler)
    }

    @Test
    fun canOpenDashboardWeb_quandoUrlForValida_deveRetornarTrue() {
        // Arrange
        val viewModel = createViewModel(dashboardWebUrl = VALID_DASHBOARD_URL)

        // Act
        val canOpen = viewModel.canOpenDashboardWeb

        // Assert
        assertTrue(canOpen)
        confirmVerified(repository, locationHandler)
    }

    @Test
    fun canOpenDashboardWeb_quandoUrlForEmBranco_deveRetornarFalse() {
        // Arrange
        val viewModel = createViewModel(dashboardWebUrl = BLANK_DASHBOARD_URL)

        // Act
        val canOpen = viewModel.canOpenDashboardWeb

        // Assert
        assertFalse(canOpen)
        confirmVerified(repository, locationHandler)
    }

    @Test
    fun onAction_OpenDashboardWeb_comUrlValida_deveChamarLocationHandler() = runTest(testDispatcher) {
        // Arrange
        val viewModel = createViewModel(dashboardWebUrl = VALID_DASHBOARD_URL)

        // Act
        viewModel.onAction(ProfileAction.OpenDashboardWeb)

        // Assert
        verify(exactly = 1) { locationHandler.openWebUrl(VALID_DASHBOARD_URL) }
        coVerify(exactly = 0) { repository.getCurrentUser() }
        confirmVerified(repository, locationHandler)
    }

    @Test
    fun onAction_OpenDashboardWeb_comUrlEmBranco_deveIgnorarE_NaoChamarLocationHandler() = runTest(testDispatcher) {
        // Arrange
        val viewModel = createViewModel(dashboardWebUrl = BLANK_DASHBOARD_URL)

        // Act
        viewModel.onAction(ProfileAction.OpenDashboardWeb)

        // Assert
        verify(exactly = 0) { locationHandler.openWebUrl(any()) }
        coVerify(exactly = 0) { repository.getCurrentUser() }
        confirmVerified(repository, locationHandler)
    }

    @Test
    fun showLogoutConfirmation_quandoAcionado_deveMudarStateParaTrue() {
        // Arrange
        val viewModel = createViewModel()

        // Act
        viewModel.showLogoutConfirmation()

        // Assert
        assertTrue(viewModel.showLogoutDialog.value)
        confirmVerified(repository, locationHandler)
    }

    @Test
    fun dismissLogoutConfirmation_quandoAcionado_deveMudarStateParaFalse() {
        // Arrange
        val viewModel = createViewModel()
        viewModel.showLogoutConfirmation()
        assertTrue(viewModel.showLogoutDialog.value)

        // Act
        viewModel.dismissLogoutConfirmation()

        // Assert
        assertFalse(viewModel.showLogoutDialog.value)
        confirmVerified(repository, locationHandler)
    }

    private fun createViewModel(
        dashboardWebUrl: String = VALID_DASHBOARD_URL
    ): ProfileViewModel {
        return ProfileViewModel(
            repository = repository,
            locationHandler = locationHandler,
            dashboardWebUrl = dashboardWebUrl
        )
    }

    private fun createUser(): UserDTO {
        return UserDTO(
            email = "usuario@teste.com",
            fullName = "Fulano da Silva",
            roles = listOf("USER")
        )
    }

    private fun stubAndroidLog() {
        every { Log.d(any<String>(), any<String>()) } returns 0
        every { Log.i(any<String>(), any<String>()) } returns 0
        every { Log.w(any<String>(), any<String>()) } returns 0
        every { Log.w(any<String>(), any<String>(), any<Throwable>()) } returns 0
        every { Log.e(any<String>(), any<String>()) } returns 0
        every { Log.e(any<String>(), any<String>(), any<Throwable>()) } returns 0
    }

    private companion object {
        private const val VALID_DASHBOARD_URL = "https://dashboard.exemplo.com"
        private const val BLANK_DASHBOARD_URL = "   "
    }
}