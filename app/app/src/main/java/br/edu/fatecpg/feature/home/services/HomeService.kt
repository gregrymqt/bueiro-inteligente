package br.edu.fatecpg.feature.home.services

import br.edu.fatecpg.feature.home.dto.HomeResponseDTO
import retrofit2.http.GET

interface HomeService {
    /**
     * Consome os dados públicos/administrativos da Home configurados no painel Web.
     */
    @GET("home")
    suspend fun getHomeContent(): Result<HomeResponseDTO>
}
