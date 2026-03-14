import logging
from typing import Optional
from .interfaces import IDrainRepository
from .dto import DrainStatusDTO

logger = logging.getLogger(__name__)

class DrainRepository(IDrainRepository):
    # Recebemos as conexões já instanciadas lá da nossa pasta /core
    def __init__(self, db_client, cache_client):
        self._db = db_client
        self._cache = cache_client

    async def save_sensor_data(self, data: DrainStatusDTO) -> None:
        """
        Implementa o contrato IDrainRepository.
        Salva o dado fresco no Redis e o histórico no Supabase/Firebase.
        """
        # 1. ATUALIZAR O REDIS
        cache_key = f"bueiro:{data.id_bueiro}:status"
        
        try:
            # O model_dump_json() do Pydantic já converte o DTO para uma string JSON válida
            await self._cache.set(cache_key, data.model_dump_json())
            # Definir um tempo de expiração (TTL) no Redis se o sensor parar de enviar
            await self._cache.expire(cache_key, 3600) # Expira em 1 hora
        except Exception as e:
            logger.error(f"Erro ao salvar no cache o bueiro {data.id_bueiro}: {e}")

        # 2. INSERIR NO BANCO SQL (Histórico)
        db_payload = {
            "id_bueiro": data.id_bueiro,
            "distancia_cm": data.distancia_cm,
            "nivel_obstrucao": data.nivel_obstrucao,
            "status": data.status,
            "latitude": data.latitude,
            "longitude": data.longitude,
            "criado_em": data.ultima_atualizacao.isoformat()
        }
        
        try:
            # Inserção assíncrona no Supabase ativada e corrigida
            await self._db.table("historico_medicoes").insert(db_payload).execute()
        except Exception as e:
            logger.error(f"Erro ao inserir histórico no banco de dados para bueiro {data.id_bueiro}: {e}")
            raise Exception("Falha ao salvar medição no banco de dados") from e

    async def get_latest_status(self, bueiro_id: str) -> Optional[DrainStatusDTO]:
        """
        Busca a medição mais recente de um bueiro no Supabase como plano de fallback.
        """
        try:
            resposta_db = await self._db.table("historico_medicoes") \
                .select("*") \
                .eq("id_bueiro", bueiro_id) \
                .order("criado_em", desc=True) \
                .limit(1) \
                .execute()
            
            if not resposta_db.data:
                return None
            
            return DrainStatusDTO(**resposta_db.data[0])
            
        except Exception as e:
            logger.error(f"Erro ao buscar histórico no banco de dados para bueiro {bueiro_id}: {e}")
            raise Exception("Falha ao consultar medição no banco de dados") from e
        

    async def get_unsynced_data(self, limit: int = 100) -> list[DrainStatusDTO]:
        try:
            resposta_db = await self._db.table("historico_medicoes") \
                .select("*") \
                .eq("sincronizado_rows", False) \
                .order("criado_em", ascending=True) \
                .limit(limit) \
                .execute()
            
            return [DrainStatusDTO(**row) for row in resposta_db.data] if resposta_db.data else []
        except Exception as e:
            logger.error(f"Erro ao buscar dados não sincronizados: {e}")
            return []

    async def mark_as_synced(self, ids: list[str]) -> None:
        if not ids: return
        try:
            # Atualiza todos os IDs passados para True
            await self._db.table("historico_medicoes") \
                .update({"sincronizado_rows": True}) \
                .in_("id_bueiro", ids) \
                .execute() # Nota: Supabase requer que id_bueiro seja chave primária ou única na tabela para isso funcionar perfeitamente com UUIDs
        except Exception as e:
            logger.error(f"Erro ao marcar dados como sincronizados: {e}")    