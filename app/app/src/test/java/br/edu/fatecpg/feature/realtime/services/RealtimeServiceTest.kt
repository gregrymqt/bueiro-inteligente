package br.edu.fatecpg.feature.realtime.services

import android.util.Log
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.realtime.client.RealtimeWebSocketClient
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

class RealtimeServiceTest {

    private val client = mockk<RealtimeWebSocketClient>()

    private lateinit var service: RealtimeService

    @Before
    fun setUp() {
        mockkStatic(Log::class)
        stubAndroidLog()
        every { client.drainStatusFlow } returns MutableSharedFlow<DrainStatusDTO>()
        every { client.connectionErrorFlow } returns MutableSharedFlow<String?>()
        service = RealtimeService(client)
    }

    @After
    fun tearDown() {
        clearAllMocks()
        unmockkStatic(Log::class)
    }

    @Test
    fun connect_deveChamarClientConnect() = runTest {
        // Arrange
        val token = "token-123"

        // Act
        service.connect(token)

        // Assert
        coVerify(exactly = 1) { client.connect(token) }
    }

    @Test
    fun disconnect_deveChamarClientDisconnect() = runTest {
        // Arrange

        // Act
        service.disconnect()

        // Assert
        coVerify(exactly = 1) { client.disconnect() }
    }

    @Test
    fun joinDrain_deveChamarClientJoinDrain() = runTest {
        // Arrange
        val bueiroId = "drain-123"

        // Act
        service.joinDrain(bueiroId)

        // Assert
        coVerify(exactly = 1) { client.joinDrain(bueiroId) }
    }

    @Test
    fun leaveDrain_deveChamarClientLeaveDrain() = runTest {
        // Arrange
        val bueiroId = "drain-123"

        // Act
        service.leaveDrain(bueiroId)

        // Assert
        coVerify(exactly = 1) { client.leaveDrain(bueiroId) }
    }

    private fun stubAndroidLog() {
        every { Log.d(any<String>(), any<String>()) } returns 0
        every { Log.i(any<String>(), any<String>()) } returns 0
        every { Log.w(any<String>(), any<String>()) } returns 0
        every { Log.e(any<String>(), any<String>()) } returns 0
        every { Log.e(any<String>(), any<String>(), any<Throwable>()) } returns 0
    }
}