package br.edu.fatecpg.feature.monitoring.dto

import com.google.gson.annotations.SerializedName

data class DrainStatusDTO(
    @SerializedName("id") val id: String,
    @SerializedName("name") val name: String,
    @SerializedName("address") val address: String,
    @SerializedName("hardware_id") val hardwareId: String,
    @SerializedName("is_active") val isActive: Boolean,
    @SerializedName("status") val status: String?,
    @SerializedName("nivel_obstrucao") val nivelObstrucao: Double?,
    @SerializedName("distancia_cm") val distanciaCm: Double?,
    @SerializedName("ultima_atualizacao") val ultimaAtualizacao: String?,
    @SerializedName("latitude") val latitude: Double?,
    @SerializedName("longitude") val longitude: Double?
)
