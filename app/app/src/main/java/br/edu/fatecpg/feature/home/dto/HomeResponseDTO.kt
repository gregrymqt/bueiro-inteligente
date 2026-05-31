package br.edu.fatecpg.feature.home.dto

import com.google.gson.annotations.SerializedName

data class HomeResponseDTO(
    @SerializedName("carousels")
    val carousels: List<CarouselDTO>,
    @SerializedName("stats")
    val stats: List<StatCardDTO>
)

data class CarouselDTO(
    val id: String,
    val title: String,
    val subtitle: String?,
    @SerializedName("mobile_image_url")
    val imageUrl: String,
    @SerializedName("action_url")
    val actionUrl: String?,
    val order: Int,
    val section: String
)

data class StatCardDTO(
    val id: String,
    val title: String,
    val value: String,
    val description: String,
    @SerializedName("icon_name")
    val iconName: String,
    val color: String,
    val order: Int
)