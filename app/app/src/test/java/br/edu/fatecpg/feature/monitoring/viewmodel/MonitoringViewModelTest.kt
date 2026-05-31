package br.edu.fatecpg.feature.monitoring.viewmodel

import android.util.Log
import br.edu.fatecpg.core.navigation.LocationHandler
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
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
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class MonitoringViewModelTest {

    private val testDispatcher = UnconfinedTestDispatcher()
    private val repository = mockk<MonitoringRepository>()
    private val locationHandler = mockk<LocationHandler>(relaxed = true)
    private val realtimeRepository = mockk<RealtimeRepository>(relaxed = true)

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
    fun init_deveDispararCarregamentoInicialDeBueiros() = runTest(testDispatcher) {
        // Arrange
        val drains = createDrainList()
        val harness = createViewModel(drains)

        // Act
        advanceUntilIdle()

        // Assert
        assertEquals(MonitoringUiState.Success(drains), harness.viewModel.uiState.value)
        coVerify(exactly = 1) { repository.getAllDrains() }
        verify(exactly = 1) { realtimeRepository.alertas }
        confirmVerified(repository, locationHandler, realtimeRepository)
    }

    @Test
    fun onDrainClick_usuarioNaoLogado_deveMostrarModalDeLoginESemExpandirItem() = runTest(testDispatcher) {
        // Arrange
        val drains = createDrainList()
        val harness = createViewModel(drains)
        val drain = drains.first()
        advanceUntilIdle()

        // Act
        harness.viewModel.onDrainClick(isLoggedIn = false, drain = drain)

        // Assert
        assertTrue(harness.viewModel.showLoginDialog.value)
        assertEquals(null, harness.viewModel.expandedDrainId.value)
        coVerify(exactly = 1) { repository.getAllDrains() }
        verify(exactly = 1) { realtimeRepository.alertas }
        verify(exactly = 0) { realtimeRepository.joinDrain(any()) }
        verify(exactly = 0) { realtimeRepository.leaveDrain(any()) }
        confirmVerified(repository, locationHandler, realtimeRepository)
    }

    @Test
    fun onDrainClick_usuarioLogado_deveExpandirBueiroEEntrarNoCanalPubSub() = runTest(testDispatcher) {
        // Arrange
        val drains = createDrainList()
        val harness = createViewModel(drains)
        val drain = drains.first()
        advanceUntilIdle()

        // Act
        harness.viewModel.onDrainClick(isLoggedIn = true, drain = drain)

        // Assert
        assertEquals(drain.id, harness.viewModel.expandedDrainId.value)
        coVerify(exactly = 1) { repository.getAllDrains() }
        verify(exactly = 1) { realtimeRepository.alertas }
        verify(exactly = 1) { realtimeRepository.joinDrain(drain.id) }
        verify(exactly = 0) { realtimeRepository.leaveDrain(any()) }
        confirmVerified(repository, locationHandler, realtimeRepository)
    }

    @Test
    fun onDrainClick_cliqueNoMesmoBueiroExpandido_deveRecolherItemESairDoCanalPubSub() = runTest(testDispatcher) {
        // Arrange
        val drains = createDrainList()
        val harness = createViewModel(drains)
        val drain = drains.first()
        advanceUntilIdle()

        harness.viewModel.onDrainClick(isLoggedIn = true, drain = drain)
        assertEquals(drain.id, harness.viewModel.expandedDrainId.value)

        // Act
        harness.viewModel.onDrainClick(isLoggedIn = true, drain = drain)

        // Assert
        assertEquals(null, harness.viewModel.expandedDrainId.value)
        coVerify(exactly = 1) { repository.getAllDrains() }
        verify(exactly = 1) { realtimeRepository.alertas }
        verify(exactly = 1) { realtimeRepository.joinDrain(drain.id) }
        verify(exactly = 1) { realtimeRepository.leaveDrain(drain.id) }
        confirmVerified(repository, locationHandler, realtimeRepository)
    }

    @Test
    fun quandoReceberAlertaRTCompativelComItemExpandido_deveAtualizarItemEspecificoNaLista() = runTest(testDispatcher) {
        // Arrange
        val drains = createDrainList()
        val harness = createViewModel(drains)
        val expandedDrain = drains.first()
        val updatedAlert = createDrain(
            id = expandedDrain.id,
            name = expandedDrain.name,
            hardwareId = expandedDrain.hardwareId,
            status = "crítico",
            nivelObstrucao = 98.0,
            distanciaCm = 5.0
        )

        advanceUntilIdle()
        harness.viewModel.onDrainClick(isLoggedIn = true, drain = expandedDrain)

        // Act
        harness.alertasFlow.emit(updatedAlert)
        advanceUntilIdle()

        // Assert
        val expectedDrains = listOf(updatedAlert, drains[1])
        assertEquals(MonitoringUiState.Success(expectedDrains), harness.viewModel.uiState.value)
        assertEquals(expandedDrain.id, harness.viewModel.expandedDrainId.value)
        coVerify(exactly = 1) { repository.getAllDrains() }
        verify(exactly = 1) { realtimeRepository.alertas }
        verify(exactly = 1) { realtimeRepository.joinDrain(expandedDrain.id) }
        verify(exactly = 0) { realtimeRepository.leaveDrain(any()) }
        confirmVerified(repository, locationHandler, realtimeRepository)
    }

    @Test
    fun openDrainInMaps_comCoordenadasValidas_deveDelegarParaLocationHandler() = runTest(testDispatcher) {
        // Arrange
        val drains = createDrainList()
        val harness = createViewModel(drains)
        val drain = drains.first()
        advanceUntilIdle()

        // Act
        harness.viewModel.openDrainInMaps(drain)

        // Assert
        verify(exactly = 1) {
            locationHandler.openLocation(drain.latitude!!, drain.longitude!!, drain.name)
        }
        coVerify(exactly = 1) { repository.getAllDrains() }
        verify(exactly = 1) { realtimeRepository.alertas }
        confirmVerified(repository, locationHandler, realtimeRepository)
    }

    @Test
    fun onCleared_seHouverBueiroExpandido_deveGarantirSaidaDoCanalPubSub() = runTest(testDispatcher) {
        // Arrange
        val drains = createDrainList()
        val harness = createViewModel(drains)
        val drain = drains.first()
        advanceUntilIdle()
        harness.viewModel.onDrainClick(isLoggedIn = true, drain = drain)
        assertEquals(drain.id, harness.viewModel.expandedDrainId.value)

        // Act
        invokeOnCleared(harness.viewModel)

        // Assert
        verify(exactly = 1) { realtimeRepository.joinDrain(drain.id) }
        verify(exactly = 1) { realtimeRepository.leaveDrain(drain.id) }
        coVerify(exactly = 1) { repository.getAllDrains() }
        verify(exactly = 1) { realtimeRepository.alertas }
        confirmVerified(repository, locationHandler, realtimeRepository)
    }

    private fun createViewModel(drains: List<DrainStatusDTO>): MonitoringViewModelHarness {
        val alertasFlow = MutableSharedFlow<DrainStatusDTO>(replay = 1)
        coEvery { repository.getAllDrains() } returns Result.success(drains)
        every { realtimeRepository.alertas } returns alertasFlow

        return MonitoringViewModelHarness(
            viewModel = MonitoringViewModel(
                repository = repository,
                locationHandler = locationHandler,
                realtimeRepository = realtimeRepository
            ),
            alertasFlow = alertasFlow
        )
    }

    private fun createDrainList(): List<DrainStatusDTO> {
        return listOf(
            createDrain(
                id = "drain-1",
                name = "Bueiro Central",
                hardwareId = "hw-1",
                status = "normal",
                nivelObstrucao = 10.0,
                distanciaCm = 88.0
            ),
            createDrain(
                id = "drain-2",
                name = "Bueiro Norte",
                hardwareId = "hw-2",
                status = "alerta",
                nivelObstrucao = 70.0,
                distanciaCm = 42.0
            )
        )
    }

    private fun createDrain(
        id: String,
        name: String,
        hardwareId: String,
        status: String?,
        nivelObstrucao: Double?,
        distanciaCm: Double?
    ): DrainStatusDTO {
        return DrainStatusDTO(
            id = id,
            name = name,
            address = "Rua Principal, 123",
            hardwareId = hardwareId,
            isActive = true,
            status = status,
            nivelObstrucao = nivelObstrucao,
            distanciaCm = distanciaCm,
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

    private fun invokeOnCleared(viewModel: MonitoringViewModel) {
        val method = MonitoringViewModel::class.java.getDeclaredMethod("onCleared")
        method.isAccessible = true
        method.invoke(viewModel)
    }

    private data class MonitoringViewModelHarness(
        val viewModel: MonitoringViewModel,
        val alertasFlow: MutableSharedFlow<DrainStatusDTO>
    )
}