package br.edu.fatecpg.core.data.local

import android.util.Log
import com.google.gson.Gson
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class LocalCacheService(
    private val cacheDao: CacheDao,
    private val gson: Gson
) {
    private val ioDispatcher = Dispatchers.IO

    suspend fun <T : Any> set(key: String, value: T, expiryMillis: Long?) {
        withContext(ioDispatcher) {
            val expiresAt = expiryMillis?.let { System.currentTimeMillis() + it }
            val entity = CacheEntity(
                key = key,
                value = gson.toJson(value),
                expiresAt = expiresAt
            )

            cacheDao.insert(entity)
            Log.d("LocalCacheService", "Cache salvo para chave $key")
        }
    }

    suspend fun <T : Any> get(key: String, type: Class<T>): T? {
        return withContext(ioDispatcher) {
            val entity = cacheDao.get(key) ?: return@withContext null

            if (entity.isExpired()) {
                cacheDao.delete(entity)
                Log.d("LocalCacheService", "Cache expirado removido para chave $key")
                return@withContext null
            }

            deserialize(entity, type)
        }
    }

    suspend fun remove(key: String) {
        withContext(ioDispatcher) {
            cacheDao.delete(
                CacheEntity(
                    key = key,
                    value = "",
                    expiresAt = null
                )
            )
            Log.d("LocalCacheService", "Cache removido para chave $key")
        }
    }

    suspend fun <T : Any> getOrSet(
        key: String,
        type: Class<T>,
        expiryMillis: Long?,
        fetchFunc: suspend () -> T
    ): T {
        val cachedEntity = getCachedEntity(key)

        if (cachedEntity != null && !cachedEntity.isExpired()) {
            val cachedValue = deserialize(cachedEntity, type)
            if (cachedValue != null) {
                Log.d("LocalCacheService", "Cache hit para chave $key")
                return cachedValue
            }
        }

        return try {
            val freshValue = fetchFunc()
            set(key, freshValue, expiryMillis)
            freshValue
        } catch (exception: Exception) {
            if (cachedEntity != null) {
                val staleValue = deserialize(cachedEntity, type)
                if (staleValue != null) {
                    Log.w(
                        "LocalCacheService",
                        "Usando cache expirado para chave $key após falha de atualização",
                        exception
                    )
                    return staleValue
                }
            }

            throw exception
        }
    }

    suspend inline fun <reified T : Any> getOrSet(
        key: String,
        expiryMillis: Long?,
        noinline fetchFunc: suspend () -> T
    ): T = getOrSet(key, T::class.java, expiryMillis, fetchFunc)

    private suspend fun getCachedEntity(key: String): CacheEntity? {
        return withContext(ioDispatcher) { cacheDao.get(key) }
    }

    private fun <T : Any> deserialize(entity: CacheEntity, type: Class<T>): T? {
        return runCatching { gson.fromJson(entity.value, type) }
            .onFailure {
                Log.e("LocalCacheService", "Falha ao desserializar cache para chave ${entity.key}", it)
            }
            .getOrNull()
    }

    private fun CacheEntity.isExpired(): Boolean {
        val currentTime = System.currentTimeMillis()
        return expiresAt?.let { currentTime >= it } == true
    }
}