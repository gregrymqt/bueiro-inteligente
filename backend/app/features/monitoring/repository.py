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
            logger.info(f"Gravando status novo no Cache (Bueiro: {data.id_bueiro})")
            # O model_dump_json() do Pydantic já converte o DTO para uma string JSON válida
            await self._cache.set(cache_key, data.model_dump_json(), ttl_seconds=3600)
        except Exception as e:
            logger.error(f"Erro ao salvar no cache o bueiro {data.id_bueiro}: {str(e)}", exc_info=True)

        # 2. INSERIR NO BANCO SQL (Histórico)
        try:
            logger.info(f"Inserindo histórico de medição banco SQL (Bueiro: {data.id_bueiro})")
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
            
            # Inserção síncrona com SQLAlchemy (Commit e Add)
            self._db.add(db_record)
            self._db.commit()
            self._db.refresh(db_record)
            logger.info(f"Histórico gravado no banco de dados com sucesso (Bueiro: {data.id_bueiro})")
        except Exception as e:
            self._db.rollback()
            logger.error(f"Erro ao inserir histórico no banco de dados para bueiro {data.id_bueiro}: {str(e)}", exc_info=True)
            raise Exception("Falha ao salvar medição no banco de dados") from e

    async def get_latest_status(self, bueiro_id: str) -> Optional[DrainStatusDTO]:
        """
        Busca a medição mais recente de um bueiro no banco de dados como plano de fallback.
        """
        try:
            logger.debug(f"Buscando histórico do banco de dados (Bueiro: {bueiro_id})")
            record = self._db.query(DrainStatus)\
                .filter(DrainStatus.id_bueiro == bueiro_id)\
                .order_by(desc(DrainStatus.ultima_atualizacao))\
                .first()
            
            if not record:
                logger.debug(f"Nenhum status encontrado banco de dados (Bueiro: {bueiro_id})")
                return None
            
            return DrainStatusDTO.model_validate(record)
            
        except Exception as e:
            logger.error(f"Erro ao buscar histórico no banco de dados para bueiro {bueiro_id}: {str(e)}", exc_info=True)
            raise Exception("Falha ao consultar medição no banco de dados") from e
        

    async def get_unsynced_data(self, limit: int = 100) -> list[DrainStatusDTO]:
        try:
            logger.info(f"Buscando as requisições não sincronizadas com limite de: {limit}")
            records = self._db.query(DrainStatus)\
                .filter(DrainStatus.sincronizado_rows == False)\
                .order_by(DrainStatus.ultima_atualizacao.asc())\
                .limit(limit)\
                .all()
            
            logger.info(f"Foram encontradas {len(records)} linhas pendentes de sincronização")
            return [DrainStatusDTO.model_validate(row) for row in records]
        except Exception as e:
            logger.error(f"Erro ao buscar dados não sincronizados para Rows: {str(e)}", exc_info=True)
            return []

    async def mark_as_synced(self, ids: list[str]) -> None:
        """
        No banco de dados com a modelagem corrigida de histórico de medições,
        estamos atualizando o status dos bueiros passados na lista usando `id_bueiro`.
        Se estivermos agrupando por ID do registro na tabela na sua lógica futuramente,
        esta query deve mudar de `id_bueiro` para `id`.
        """
        if not ids: 
            return
            
        try:
            logger.info(f"Marcando {len(ids)} ids de registros de bueiro(s) como já sincronizados.")
            # Atualiza todos os IDs (de bueiros) passados como sincronizados
            self._db.query(DrainStatus)\
                .filter(DrainStatus.id_bueiro.in_(ids))\
                .update({"sincronizado_rows": True}, synchronize_session=False)
            
            self._db.commit()
            logger.info(f"Status de registros alterados com sucesso para sincronizados.")
        except Exception as e:
            self._db.rollback()
            logger.error(f"Erro crítico no banco de dados ao tentar marcar id(s) como sincronizados ({ids}): {str(e)}", exc_info=True)
            raise    