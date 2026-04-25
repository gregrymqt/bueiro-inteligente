package br.edu.fatecpg.core.data.local

import androidx.room.Database
import androidx.room.RoomDatabase

@Database(entities = [CacheEntity::class], version = 1, exportSchema = false)
abstract class AppDatabase : RoomDatabase() {
    abstract fun cacheDao(): CacheDao
}