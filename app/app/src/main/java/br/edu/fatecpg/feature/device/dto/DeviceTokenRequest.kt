package br.edu.fatecpg.feature.device.dto

import com.google.gson.annotations.SerializedName

data class DeviceTokenRequest(
    @SerializedName("fcmToken") val fcmToken: String
)
