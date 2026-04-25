package br.edu.fatecpg.core.data.local

import android.util.Log
import com.google.gson.Gson
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlinx.coroutines.withContext
import java.lang.reflect.Type
import java.util.concurrent.ConcurrentHashMap
import kotlin.reflect.javaType
import kotlin.reflect.typeOf

class LocalCacheService(
    private val cacheDao: CacheDao,
    private val gson: Gson
) {
    private val ioDispatcher = Dispatchers.IO
    private val mutexes = ConcurrentHashMap<String, Mutex>()

    suspend fun clearExpired() {
        withContext(ioDispatcher) {
            try {
                cacheDao.clearExpired(System.currentTimeMillis())
                Log.d("LocalCacheService", "Cache expirado removido com sucesso")
            } catch (e: Exception) {
                Log.e("LocalCacheService", "Erro ao limpar cache expirado", e)
            }
        }
    }

    suspend fun <T : Any> set(key: String, value: T, expiryMillis: Long?) {
        withContext(ioDispatcher) {
            try {
                val expiresAt = expiryMillis?.let { System.currentTimeMillis() + it }
                val entity = CacheEntity(
                    key = key,
                    value = gson.toJson(value),
                    expiresAt = expiresAt
                )

                cacheDao.insert(entity)
                Log.d("LocalCacheService", "Cache salvo para chave $key")
            } catch (e: Exception) {
                Log.e("LocalCacheService", "Erro ao salvar cache para a chave $key", e)
            }
        }
    }

    suspend fun <T : Any> get(key: String, type: Type): T? {
        return withContext(ioDispatcher) {
            try {
                val entity = cacheDao.get(key) ?: return@withContext null

                if (entity.isExpired()) {
                    cacheDao.delete(entity)
                    Log.d("LocalCacheService", "Cache expirado removido para chave $key")
                    return@withContext null
                }

                deserialize(entity, type)
            } catch (e: Exception) {
                Log.e("LocalCacheService", "Erro ao buscar cache para a chave $key", e)
                null
            }
        }
    }

    suspend fun remove(key: String) {
        withContext(ioDispatcher) {
            try {
                cacheDao.delete(
                    CacheEntity(
                        key = key,
                        value = "",
                        expiresAt = null
                    )
                )
                Log.d("LocalCacheService", "Cache removido para chave $key")
            } catch (e: Exception) {
                Log.e("LocalCacheService", "Erro ao remover cache para a chave $key", e)
            }
        }
    }

    suspend fun <T : Any> getOrSet(
        key: String,
        type: Type,
        expiryMillis: Long?,
        fetchFunc: suspend () -> T
    ): T {
        val mutex = mutexes.getOrPut(key) { Mutex() }

        return mutex.withLock {
            val cachedEntity = getCachedEntity(key)

            if (cachedEntity != null && !cachedEntity.isExpired()) {
                val cachedValue = deserialize(cachedEntity, type)
                if (cachedValue != null) {
                    Log.d("LocalCacheService", "Cache hit para chave $key")
                    return@withLock cachedValue
                }
            }

            try {
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
                        return@withLock staleValue
                    }
                }

                throw exception
            }
        }
    }

    @OptIn(ExperimentalStdlibApi::class)
    suspend inline fun <reified T : Any> getOrSet(
        key: String,
        expiryMillis: Long?,
        noinline fetchFunc: suspend () -> T
    ): T = getOrSet(key, typeOf<T>().javaType, expiryMillis, fetchFunc)

    private suspend fun getCachedEntity(key: String): CacheEntity? {
        return withContext(ioDispatcher) {
            try {
                cacheDao.get(key)
            } catch (e: Exception) {
                Log.e("LocalCacheService", "Erro ao buscar entidade de cache para a chave $key", e)
                null
            }
        }
    }

    private fun <T : Any> deserialize(entity: CacheEntity, type: Type): T? {
        return runCatching { gson.fromJson<T>(entity.value, type) }
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