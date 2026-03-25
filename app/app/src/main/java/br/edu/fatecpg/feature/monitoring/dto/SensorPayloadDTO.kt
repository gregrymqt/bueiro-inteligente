package br.edu.fatecpg.feature.monitoring.dto

import com.google.gson.annotations.SerializedName

data class SensorPayloadDTO(
    @SerializedName("id_bueiro") val idBueiro: String,
    @SerializedName("distancia_cm") val distanciaCm: Double,
    val latitude: Double?,
    val longitude: Double?
)
