import logging
from typing import Optional
from sqlalchemy.orm import Session
from sqlalchemy import desc
from .interfaces import IDrainRepository
from .dto import DrainStatusDTO
from .models import DrainStatus
from app.features.cache.interfaces import ICacheService

logger = logging.getLogger(__name__)

class DrainRepository(IDrainRepository):
    # Recebemos as conexões já instanciadas lá da nossa pasta /core
    def __init__(self, db_client: Session, cache_service: ICacheService):
        self._db = db_client
        self._cache = cache_service

    async def save_sensor_data(self, data: DrainStatusDTO) -> None:
        """
        Implementa o contrato IDrainRepository.
        Salva o dado fresco no Redis e o histórico no Supabase/Firebase.
        """
        # 1. ATUALIZAR O REDIS
        cache_key = f"bueiro:{data.id_bueiro}:status"
        
        try:
            # O model_dump_json() do Pydantic já converte o DTO para uma string JSON válida
            await self._cache.set(cache_key, data.model_dump_json(), ttl_seconds=3600)
        except Exception as e:
            logger.error(f"Erro ao salvar no cache o bueiro {data.id_bueiro}: {e}")

        # 2. INSERIR NO BANCO SQL (Histórico)
        # Convertemos o DTO em uma instância do Model SQLAlchemy
        db_record = DrainStatus(
            id_bueiro=data.id_bueiro,
            distancia_cm=data.distancia_cm,
            nivel_obstrucao=data.nivel_obstrucao,
            status=data.status,
            latitude=data.latitude,
            longitude=data.longitude,
            ultima_atualizacao=data.ultima_atualizacao,
            sincronizado_rows=False
        )
        
        try:
            # Inserção síncrona com SQLAlchemy (Commit e Add)
            self._db.add(db_record)
            self._db.commit()
            self._db.refresh(db_record)
        except Exception as e:
            self._db.rollback()
            logger.error(f"Erro ao inserir histórico no banco de dados para bueiro {data.id_bueiro}: {e}")
            raise Exception("Falha ao salvar medição no banco de dados") from e

    async def get_latest_status(self, bueiro_id: str) -> Optional[DrainStatusDTO]:
        """
        Busca a medição mais recente de um bueiro no banco de dados como plano de fallback.
        """
        try:
            record = self._db.query(DrainStatus)\
                .filter(DrainStatus.id_bueiro == bueiro_id)\
                .order_by(desc(DrainStatus.ultima_atualizacao))\
                .first()
            
            if not record:
                return None
            
            return DrainStatusDTO.model_validate(record)
            
        except Exception as e:
            logger.error(f"Erro ao buscar histórico no banco de dados para bueiro {bueiro_id}: {e}")
            raise Exception("Falha ao consultar medição no banco de dados") from e
        

    async def get_unsynced_data(self, limit: int = 100) -> list[DrainStatusDTO]:
        try:
            records = self._db.query(DrainStatus)\
                .filter(DrainStatus.sincronizado_rows == False)\
                .order_by(DrainStatus.ultima_atualizacao.asc())\
                .limit(limit)\
                .all()
            
            return [DrainStatusDTO.model_validate(row) for row in records]
        except Exception as e:
            logger.error(f"Erro ao buscar dados não sincronizados: {e}")
            return []

    async def mark_as_synced(self, ids: list[str]) -> None:
        """
        No banco de dados com a modelagem corrigida de histórico de medições,
        estamos atualizando o status dos bueiros passados na lista usando `id_bueiro`.
        Se estivermos agrupando por ID do registro na tabela na sua lógica futuramente,
        esta query deve mudar de `id_bueiro` para `id`.
        """
        if not ids: return
        try:
            # Atualiza todos os IDs (de bueiros) passados como sincronizados
            self._db.query(DrainStatus)\
                .filter(DrainStatus.id_bueiro.in_(ids))\
                .update({"sincronizado_rows": True}, synchronize_session=False)
            
            self._db.commit()
        except Exception as e:
            self._db.rollback()
            logger.error(f"Erro ao marcar dados como sincronizados: {e}")    