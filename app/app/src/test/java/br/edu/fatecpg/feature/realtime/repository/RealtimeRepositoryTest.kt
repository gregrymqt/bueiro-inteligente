package br.edu.fatecpg.feature.realtime.repository

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.services.RealtimeService
import io.mockk.clearAllMocks
import io.mockk.coVerify
import io.mockk.every
import io.mockk.mockk
import io.mockk.mockkStatic
import io.mockk.unmockkStatic
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.test.runTest
import org.junit.After
import org.junit.Assert.assertSame
import org.junit.Before
import org.junit.Test

class RealtimeRepositoryTest {

    private val realtimeService = mockk<RealtimeService>()

    private lateinit var repository: RealtimeRepository

    @Before
    fun setUp() {
        mockkStatic(Log::class)
        stubAndroidLog()

        val alertasFlow = MutableSharedFlow<DrainStatusDTO>()
        val connectionErrorFlow = MutableSharedFlow<String?>()
        every { realtimeService.alertas } returns alertasFlow
        every { realtimeService.connectionError } returns connectionErrorFlow

        repository = RealtimeRepository(realtimeService)
    }

    @After
    fun tearDown() {
        clearAllMocks()
        unmockkStatic(Log::class)
    }

    @Test
    fun connect_deveEncaminharComandoParaORealtimeService() = runTest {
        // Arrange
        val token = "token-123"

        // Act
        repository.connect(token)

        // Assert
        coVerify(exactly = 1) { realtimeService.connect(token) }
    }

    @Test
    fun disconnect_deveEncaminharComandoParaORealtimeService() = runTest {
        // Arrange

        // Act
        repository.disconnect()

        // Assert
        coVerify(exactly = 1) { realtimeService.disconnect() }
    }

    @Test
    fun joinDrain_deveEncaminharIDDoBueiroParaORealtimeService() = runTest {
        // Arrange
        val bueiroId = "drain-123"

        // Act
        repository.joinDrain(bueiroId)

        // Assert
        coVerify(exactly = 1) { realtimeService.joinDrain(bueiroId) }
    }

    @Test
    fun leaveDrain_deveEncaminharIDDoBueiroParaORealtimeService() = runTest {
        // Arrange
        val bueiroId = "drain-123"

        // Act
        repository.leaveDrain(bueiroId)

        // Assert
        coVerify(exactly = 1) { realtimeService.leaveDrain(bueiroId) }
    }

    private fun stubAndroidLog() {
        every { Log.d(any<String>(), any<String>()) } returns 0
        every { Log.i(any<String>(), any<String>()) } returns 0
        every { Log.w(any<String>(), any<String>()) } returns 0
        every { Log.e(any<String>(), any<String>()) } returns 0
        every { Log.e(any<String>(), any<String>(), any<Throwable>()) } returns 0
    }
}