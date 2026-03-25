package br.edu.fatecpg.feature.auth.dto

import com.google.gson.annotations.SerializedName

data class UserDTO(
    val username: String,
    @SerializedName("full_name") val fullName: String,
    val roles: List<String>
)
