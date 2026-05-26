package br.edu.fatecpg.feature.monitoring.dto

import com.google.gson.annotations.SerializedName

data class DrainStatusDTO(
    @SerializedName("id", alternate = ["Id"])
    val id: String?,

    @SerializedName("name", alternate = ["Name", "nome"])
    val name: String?,

    @SerializedName("address", alternate = ["Address", "endereco"])
    val address: String?,

    @SerializedName("id_bueiro", alternate = ["hardware_id", "idBueiro", "IdBueiro"])
    val hardwareId: String,

    @SerializedName("is_active", alternate = ["isActive", "IsActive"])
    val isActive: Boolean? = false,

    @SerializedName("status", alternate = ["Status"])
    val status: String?,

    @SerializedName("nivel_obstrucao", alternate = ["nivelObstrucao", "NivelObstrucao"])
    val nivelObstrucao: Double?,

    @SerializedName("distancia_cm", alternate = ["distanciaCm", "DistanciaCm"])
    val distanciaCm: Double?,

    @SerializedName("ultima_atualizacao", alternate = ["ultimaAtualizacao", "UltimaAtualizacao"])
    val ultimaAtualizacao: String?,

    @SerializedName("latitude", alternate = ["Latitude"])
    val latitude: Double?,

    @SerializedName("longitude", alternate = ["Longitude"])
    val longitude: Double?
)
