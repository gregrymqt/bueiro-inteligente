package br.edu.fatecpg.feature.monitoring.dto

import com.google.gson.annotations.SerializedName

data class DrainStatusDTO(
    @SerializedName("id", alternate = ["Id", "idBueiro", "IdBueiro"])
    val id: String,

    @SerializedName("name", alternate = ["Name", "nome", "Nome"])
    val name: String,

    @SerializedName("address", alternate = ["Address", "endereco", "Endereco"])
    val address: String,

    @SerializedName("hardwareId", alternate = ["hardware_id", "id_bueiro", "idBueiro", "IdBueiro"])
    val hardwareId: String,

    @SerializedName("isActive", alternate = ["is_active", "IsActive"])
    val isActive: Boolean,

    @SerializedName("status", alternate = ["Status"])
    val status: String?,

    @SerializedName("nivelObstrucao", alternate = ["nivel_obstrucao", "NivelObstrucao"])
    val nivelObstrucao: Double?,

    @SerializedName("distanciaCm", alternate = ["distancia_cm", "DistanciaCm"])
    val distanciaCm: Double?,

    @SerializedName("ultimaAtualizacao", alternate = ["ultima_atualizacao", "UltimaAtualizacao"])
    val ultimaAtualizacao: String?,

    @SerializedName("latitude", alternate = ["Latitude"])
    val latitude: Double?,

    @SerializedName("longitude", alternate = ["Longitude"])
    val longitude: Double?
)
