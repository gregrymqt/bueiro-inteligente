package br.edu.fatecpg.core.data.local

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "cache_entries")
data class CacheEntity(
    @PrimaryKey
    @ColumnInfo(name = "cache_key")
    val key: String,
    @ColumnInfo(name = "cache_value")
    val value: String,
    @ColumnInfo(name = "expires_at")
    val expiresAt: Long? = null
)