package br.edu.fatecpg.feature.home.services

import br.edu.fatecpg.feature.home.dto.HomeResponseDTO
import retrofit2.http.GET

interface HomeService {
    /**
     * Consome os dados públicos/administrativos da Home configurados no painel Web.
     */
    @GET("api/v1/home")
    suspend fun getHomeContent(): HomeResponseDTO
}