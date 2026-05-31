package br.edu.fatecpg.feature.monitoring.repository

import android.util.Log
import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import io.mockk.clearAllMocks
import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.confirmVerified
import io.mockk.every
import io.mockk.mockk
import io.mockk.mockkStatic
import io.mockk.unmockkStatic
import kotlinx.coroutines.test.runTest
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertSame
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class MonitoringRepositoryTest {

    private val monitoringService = mockk<MonitoringService>()
    private val localCacheService = mockk<LocalCacheService>()

    private val repository = MonitoringRepository(monitoringService, localCacheService)

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
    fun getAllDrains_comSucessoNaRede_deveAtualizarCacheGeralERetornarSuccessComLista() = runTest {
        // Arrange
        val expectedDrains = createDrainList()
        var cacheLambdaResult: Array<DrainStatusDTO>? = null

        coEvery { monitoringService.getAllDrains() } returns Result.success(expectedDrains)
        coEvery {
            localCacheService.getOrSet<Array<DrainStatusDTO>>(
                ALL_DRAINS_CACHE_KEY,
                any(),
                ALL_DRAINS_CACHE_TTL_MILLIS,
                any()
            )
        } coAnswers {
            val fetchFunc = arg<suspend () -> Array<DrainStatusDTO>>(3)
            val fetchedArray = fetchFunc()
            cacheLambdaResult = fetchedArray
            fetchedArray
        }

        // Act
        val result = repository.getAllDrains()

        // Assert
        assertTrue(result.isSuccess)
        assertEquals(expectedDrains, result.getOrNull())
        assertEquals(expectedDrains, cacheLambdaResult?.toList())
        coVerify(exactly = 1) { monitoringService.getAllDrains() }
        coVerify(exactly = 1) {
            localCacheService.getOrSet<Array<DrainStatusDTO>>(
                ALL_DRAINS_CACHE_KEY,
                any(),
                ALL_DRAINS_CACHE_TTL_MILLIS,
                any()
            )
        }
        confirmVerified(monitoringService, localCacheService)
    }

    @Test
    fun getAllDrains_comFalhaNaRede_deveRetornarResultFailure() = runTest {
        // Arrange
        val expectedException = RuntimeException("Falha ao buscar a lista de bueiros")
        coEvery { monitoringService.getAllDrains() } returns Result.failure(expectedException)

        // Act
        val result = repository.getAllDrains()

        // Assert
        assertTrue(result.isFailure)
        assertSame(expectedException, result.exceptionOrNull())
        coVerify(exactly = 1) { monitoringService.getAllDrains() }
        coVerify(exactly = 0) {
            localCacheService.getOrSet<Array<DrainStatusDTO>>(any(), any(), any(), any())
        }
        confirmVerified(monitoringService, localCacheService)
    }

    @Test
    fun getDrainStatus_comSucessoNaRede_deveAtualizarCacheDoIdEspecificoERetornarSuccess() = runTest {
        // Arrange
        val bueiroId = "bueiro-123"
        val expectedDrain = createDrainStatus(id = bueiroId, status = "alerta")
        var cacheLambdaResult: DrainStatusDTO? = null

        coEvery { monitoringService.getDrainStatus(bueiroId) } returns Result.success(expectedDrain)
        coEvery {
            localCacheService.getOrSet<DrainStatusDTO>(
                drainStatusCacheKeyForTest(bueiroId),
                any(),
                DRAIN_STATUS_CACHE_TTL_MILLIS,
                any()
            )
        } coAnswers {
            val fetchFunc = arg<suspend () -> DrainStatusDTO>(3)
            val fetchedDrain = fetchFunc()
            cacheLambdaResult = fetchedDrain
            fetchedDrain
        }

        // Act
        val result = repository.getDrainStatus(bueiroId)

        // Assert
        assertTrue(result.isSuccess)
        assertEquals(expectedDrain, result.getOrNull())
        assertEquals(expectedDrain, cacheLambdaResult)
        coVerify(exactly = 1) { monitoringService.getDrainStatus(bueiroId) }
        coVerify(exactly = 1) {
            localCacheService.getOrSet<DrainStatusDTO>(
                drainStatusCacheKeyForTest(bueiroId),
                any(),
                DRAIN_STATUS_CACHE_TTL_MILLIS,
                any()
            )
        }
        confirmVerified(monitoringService, localCacheService)
    }

    @Test
    fun getDrainStatus_comFalhaNaRede_deveRetornarResultFailure() = runTest {
        // Arrange
        val bueiroId = "bueiro-123"
        val expectedException = RuntimeException("Falha ao buscar o status do bueiro")
        coEvery { monitoringService.getDrainStatus(bueiroId) } returns Result.failure(expectedException)

        // Act
        val result = repository.getDrainStatus(bueiroId)

        // Assert
        assertTrue(result.isFailure)
        assertSame(expectedException, result.exceptionOrNull())
        coVerify(exactly = 1) { monitoringService.getDrainStatus(bueiroId) }
        coVerify(exactly = 0) {
            localCacheService.getOrSet<DrainStatusDTO>(any(), any(), any(), any())
        }
        confirmVerified(monitoringService, localCacheService)
    }

    private fun createDrainList(): List<DrainStatusDTO> {
        return listOf(
            createDrainStatus(id = "bueiro-123", status = "alerta"),
            createDrainStatus(id = "bueiro-456", status = "normal")
        )
    }

    private fun createDrainStatus(
        id: String,
        status: String?
    ): DrainStatusDTO {
        return DrainStatusDTO(
            id = id,
            name = "Bueiro $id",
            address = "Rua Principal, 123",
            hardwareId = "HW-$id",
            isActive = true,
            status = status,
            nivelObstrucao = 87.5,
            distanciaCm = 14.2,
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

    private companion object {
        private const val ALL_DRAINS_CACHE_KEY = "monitoring:drains:all"
        private const val ALL_DRAINS_CACHE_TTL_MILLIS = 60 * 60 * 1000L
        private const val DRAIN_STATUS_CACHE_TTL_MILLIS = 60 * 60 * 1000L

        private fun drainStatusCacheKeyForTest(bueiroId: String): String = "monitoring:drains:$bueiroId"
    }
}