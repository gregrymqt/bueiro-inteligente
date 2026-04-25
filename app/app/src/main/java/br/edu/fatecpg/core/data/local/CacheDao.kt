package br.edu.fatecpg.core.data.local

import androidx.room.Dao
import androidx.room.Delete
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query

@Dao
interface CacheDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(entity: CacheEntity)

    @Delete
    suspend fun delete(entity: CacheEntity)

    @Query("SELECT * FROM cache_entries WHERE cache_key = :key LIMIT 1")
    suspend fun get(key: String): CacheEntity?

    @Query("DELETE FROM cache_entries WHERE expires_at IS NOT NULL AND expires_at <= :currentTime")
    suspend fun clearExpired(currentTime: Long)
}