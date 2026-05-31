package br.edu.fatecpg.feature.home.viewmodel

import android.util.Log
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.core.notifications.NotificationHelper
import br.edu.fatecpg.feature.device.repository.DeviceRepository
import br.edu.fatecpg.feature.home.dto.CarouselDTO
import br.edu.fatecpg.feature.home.dto.HomeResponseDTO
import br.edu.fatecpg.feature.home.dto.StatCardDTO
import br.edu.fatecpg.feature.home.repository.HomeRepository
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import com.google.firebase.messaging.FirebaseMessaging
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
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.test.UnconfinedTestDispatcher
import kotlinx.coroutines.test.advanceUntilIdle
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class HomeViewModelTest {

    private val realtimeRepository = mockk<RealtimeRepository>(relaxed = true)
    private val homeRepository = mockk<HomeRepository>()
    private val deviceRepository = mockk<DeviceRepository>()
    private val tokenManager = mockk<TokenManager>()
    private val notificationHelper = mockk<NotificationHelper>(relaxed = true)
    private val firebaseMessaging = mockk<FirebaseMessaging>(relaxed = true)

    @Before
    fun setUp() {
        Dispatchers.setMain(UnconfinedTestDispatcher())
        mockkStatic(Log::class)
        mockkStatic(FirebaseMessaging::class)
        stubAndroidLog()
        every { FirebaseMessaging.getInstance() } returns firebaseMessaging
        every { tokenManager.getToken() } returns ""
    }

    @After
    fun tearDown() {
        clearAllMocks()
        unmockkStatic(Log::class)
        unmockkStatic(FirebaseMessaging::class)
        Dispatchers.resetMain()
    }

    @Test
    fun init_deveConectarNoWebSocketEBuscarDadosIniciais() = runTest {
        // Arrange
        val expectedHomeContent = createHomeResponse()
        val harness = createViewModel(Result.success(expectedHomeContent))

        // Act
        advanceUntilIdle()

        // Assert
        assertEquals(HomeUiState.Success(expectedHomeContent), harness.viewModel.uiState.value)
        verifyCommonInitInteractions(expectedHomeCalls = 1)
    }

    @Test
    fun loadHomeContent_comSucesso_deveDefinirUiStateComoSuccess() = runTest {
        // Arrange
        val expectedHomeContent = createHomeResponse()
        val harness = createViewModel(Result.success(expectedHomeContent))

        // Act
        harness.viewModel.loadHomeContent()
        advanceUntilIdle()

        // Assert
        assertEquals(HomeUiState.Success(expectedHomeContent), harness.viewModel.uiState.value)
        verifyCommonInitInteractions(expectedHomeCalls = 2)
    }

    @Test
    fun loadHomeContent_comFalha_deveDefinirUiStateComoErrorComMensagem() = runTest {
        // Arrange
        val expectedMessage = "Falha ao carregar o conteúdo da Home"
        val harness = createViewModel(Result.failure(RuntimeException(expectedMessage)))

        // Act
        harness.viewModel.loadHomeContent()
        advanceUntilIdle()

        // Assert
        assertEquals(HomeUiState.Error(expectedMessage), harness.viewModel.uiState.value)
        verifyCommonInitInteractions(expectedHomeCalls = 2)
    }

    @Test
    fun quandoReceberAlertaCriticoViaWebSocket_deveAtualizarActiveAlertEDispararNotificacaoNativa() = runTest {
        // Arrange
        val expectedHomeContent = createHomeResponse()
        val harness = createViewModel(Result.success(expectedHomeContent))
        val criticalAlert = createDrainStatus(status = "crítico")

        // Act
        harness.alertasFlow.emit(criticalAlert)
        advanceUntilIdle()

        // Assert
        assertEquals(criticalAlert, harness.viewModel.activeAlert.value)
        verifyCommonInitInteractions(expectedHomeCalls = 1, expectedCriticalAlert = criticalAlert)
    }

    @Test
    fun quandoReceberErroDeConexaoViaWebSocket_deveAtualizarConnectionError() = runTest {
        // Arrange
        val expectedHomeContent = createHomeResponse()
        val harness = createViewModel(Result.success(expectedHomeContent))
        val expectedError = "WebSocket desconectado"

        // Act
        harness.connectionErrorFlow.emit(expectedError)
        advanceUntilIdle()

        // Assert
        assertEquals(expectedError, harness.viewModel.connectionError.value)
        verifyCommonInitInteractions(expectedHomeCalls = 1)
    }

    @Test
    fun dismissAlert_quandoChamado_deveLimparOAlertaAtivo() = runTest {
        // Arrange
        val expectedHomeContent = createHomeResponse()
        val harness = createViewModel(Result.success(expectedHomeContent))
        val alert = createDrainStatus(status = "alerta")

        harness.alertasFlow.emit(alert)
        advanceUntilIdle()
        assertEquals(alert, harness.viewModel.activeAlert.value)

        // Act
        harness.viewModel.dismissAlert()

        // Assert
        assertEquals(null, harness.viewModel.activeAlert.value)
        verifyCommonInitInteractions(expectedHomeCalls = 1)
    }

    private fun createViewModel(homeResult: Result<HomeResponseDTO>): ViewModelHarness {
        val alertasFlow = MutableSharedFlow<DrainStatusDTO>(replay = 1)
        val connectionErrorFlow = MutableSharedFlow<String?>(replay = 1)

        every { realtimeRepository.alertas } returns alertasFlow
        every { realtimeRepository.connectionError } returns connectionErrorFlow
        coEvery { homeRepository.getHomeContent() } returns homeResult

        return ViewModelHarness(
            viewModel = HomeViewModel(
                realtimeRepository = realtimeRepository,
                homeRepository = homeRepository,
                deviceRepository = deviceRepository,
                tokenManager = tokenManager,
                notificationHelper = notificationHelper
            ),
            alertasFlow = alertasFlow,
            connectionErrorFlow = connectionErrorFlow
        )
    }

    private fun verifyCommonInitInteractions(
        expectedHomeCalls: Int,
        expectedCriticalAlert: DrainStatusDTO? = null
    ) {
        verify(exactly = 1) { realtimeRepository.connect("") }
        verify(exactly = 1) { realtimeRepository.alertas }
        verify(exactly = 1) { realtimeRepository.connectionError }
        verify(exactly = 2) { tokenManager.getToken() }
        coVerify(exactly = 0) { deviceRepository.registerToken(any()) }

        if (expectedCriticalAlert != null) {
            verify(exactly = 1) { notificationHelper.showCriticalNotification(expectedCriticalAlert) }
        } else {
            verify(exactly = 0) { notificationHelper.showCriticalNotification(any()) }
        }

        coVerify(exactly = expectedHomeCalls) { homeRepository.getHomeContent() }
        confirmVerified(
            realtimeRepository,
            homeRepository,
            deviceRepository,
            tokenManager,
            notificationHelper
        )
    }

    private fun createHomeResponse(): HomeResponseDTO {
        return HomeResponseDTO(
            carousels = listOf(
                CarouselDTO(
                    id = "carousel-1",
                    title = "Bem-vindo",
                    subtitle = "Resumo da plataforma",
                    imageUrl = "https://cdn.exemplo.com/carousel-1.png",
                    actionUrl = "https://exemplo.com/acao",
                    order = 1,
                    section = "home"
                )
            ),
            stats = listOf(
                StatCardDTO(
                    id = "stat-1",
                    title = "Bueiros monitorados",
                    value = "12",
                    description = "Unidades ativas",
                    iconName = "analytics",
                    color = "#0F766E",
                    order = 1
                )
            )
        )
    }

    private fun createDrainStatus(status: String): DrainStatusDTO {
        return DrainStatusDTO(
            id = "drain-1",
            name = "Bueiro Central",
            address = "Rua Principal, 123",
            hardwareId = "HW-001",
            isActive = true,
            status = status,
            nivelObstrucao = 95.0,
            distanciaCm = 12.5,
            ultimaAtualizacao = "2026-05-31T12:00:00Z",
            latitude = -23.55052,
            longitude = -46.63331
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

    private data class ViewModelHarness(
        val viewModel: HomeViewModel,
        val alertasFlow: MutableSharedFlow<DrainStatusDTO>,
        val connectionErrorFlow: MutableSharedFlow<String?>
    )
}