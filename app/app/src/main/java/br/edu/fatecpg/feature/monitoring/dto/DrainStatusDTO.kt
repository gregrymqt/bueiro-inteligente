package br.edu.fatecpg.feature.monitoring.dto

import com.google.gson.annotations.SerializedName

data class DrainStatusDTO(
    @SerializedName("id_bueiro") val idBueiro: String,
    @SerializedName("distancia_cm") val distanciaCm: Double,
    @SerializedName("nivel_obstrucao") val nivelObstrucao: Double,

    // CORREÇÃO: Tipo alterado para Nullable para suportar inconsistências do banco
    val status: String?,

    val latitude: Double?,
    val longitude: Double?,
    @SerializedName("ultima_atualizacao") val ultimaAtualizacao: String
)