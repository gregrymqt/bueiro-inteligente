package br.edu.fatecpg.feature.home.repository

import br.edu.fatecpg.core.data.local.LocalCacheService
import br.edu.fatecpg.feature.home.dto.CarouselDTO
import br.edu.fatecpg.feature.home.dto.HomeResponseDTO
import br.edu.fatecpg.feature.home.dto.StatCardDTO
import br.edu.fatecpg.feature.home.services.HomeService
import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.confirmVerified
import io.mockk.mockk
import kotlinx.coroutines.test.runTest
import org.junit.Assert.assertEquals
import org.junit.Assert.assertSame
import org.junit.Assert.assertTrue
import org.junit.Test

class HomeRepositoryTest {

    private val homeService = mockk<HomeService>()
    private val localCacheService = mockk<LocalCacheService>()

    private val repository = HomeRepository(homeService, localCacheService)

    @Test
    fun getHomeContent_comSucessoNaRede_deveSalvarNoCacheERetornarSuccessComOsDadosDoDTO() = runTest {
        // Arrange
        val expectedHomeContent = createHomeResponse()
        var cacheLambdaResult: HomeResponseDTO? = null

        coEvery { homeService.getHomeContent() } returns Result.success(expectedHomeContent)
        coEvery {
            localCacheService.getOrSet<HomeResponseDTO>(
                HOME_CONTENT_CACHE_KEY,
                any(),
                HOME_CONTENT_CACHE_TTL_MILLIS,
                any()
            )
        } coAnswers {
            val fetchLambda = arg<suspend () -> HomeResponseDTO>(3)
            val fetchedValue = fetchLambda()
            cacheLambdaResult = fetchedValue
            fetchedValue
        }

        // Act
        val result = repository.getHomeContent()

        // Assert
        assertTrue(result.isSuccess)
        assertEquals(expectedHomeContent, result.getOrNull())
        assertSame(expectedHomeContent, cacheLambdaResult)
        coVerify(exactly = 1) { homeService.getHomeContent() }
        coVerify(exactly = 1) {
            localCacheService.getOrSet<HomeResponseDTO>(
                HOME_CONTENT_CACHE_KEY,
                any(),
                HOME_CONTENT_CACHE_TTL_MILLIS,
                any()
            )
        }
        confirmVerified(homeService, localCacheService)
    }

    @Test
    fun getHomeContent_comFalhaNaRede_deveRetornarResultFailureENaoAtualizarOCacheLocal() = runTest {
        // Arrange
        val expectedException = RuntimeException("Falha ao buscar o conteúdo da Home")
        coEvery { homeService.getHomeContent() } returns Result.failure(expectedException)

        // Act
        val result = repository.getHomeContent()

        // Assert
        assertTrue(result.isFailure)
        assertSame(expectedException, result.exceptionOrNull())
        coVerify(exactly = 1) { homeService.getHomeContent() }
        coVerify(exactly = 0) {
            localCacheService.getOrSet<HomeResponseDTO>(any(), any(), any(), any())
        }
        confirmVerified(homeService, localCacheService)
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

    private companion object {
        private const val HOME_CONTENT_CACHE_KEY = "home:content"
        private const val HOME_CONTENT_CACHE_TTL_MILLIS = 24 * 60 * 60 * 1000L
    }
}