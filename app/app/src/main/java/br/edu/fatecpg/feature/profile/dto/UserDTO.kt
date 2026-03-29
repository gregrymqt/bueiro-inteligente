package br.edu.fatecpg.feature.profile.dto

import com.google.gson.annotations.SerializedName

data class UserDTO(
    @SerializedName("id") val id: String,
    @SerializedName("name") val name: String,
    @SerializedName("email") val email: String,
    @SerializedName("role") val role: String
)
