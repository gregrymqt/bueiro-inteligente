package br.edu.fatecpg.feature.auth.dto

import com.google.gson.annotations.SerializedName

data class UserDTO(
    val email: String,
    @SerializedName("full_name") val fullName: String,
    val roles: List<String>
)
