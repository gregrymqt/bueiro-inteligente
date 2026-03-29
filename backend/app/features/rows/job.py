import logging
from app.features.monitoring.repository import DrainRepository
from app.features.rows.services import RowsService
from app.features.rows.dtos import RowsAppendRequest

logger = logging.getLogger(__name__)

class RowsSyncJob:
    def __init__(self, repository: DrainRepository, rows_service: RowsService, spreadsheet_id: str, table_id: str):
        self.repository = repository
        self.rows_service = rows_service
        self.spreadsheet_id = spreadsheet_id
        self.table_id = table_id

    async def execute(self):
        """
        Executa o processo de ETL: Extrai dados nÃ£o sincronizados, Transforma no modelo do Rows e Carrega na planilha.
        """
        logger.info("Iniciando rotina de sincronizaÃ§Ã£o (ETL) com Rows.com...")
        
        # 1. ExtraÃ§Ã£o (Extract)
        unsynced_data = await self.repository.get_unsynced_data(limit=100)
        
        if not unsynced_data:
            logger.info("Nenhum dado novo para sincronizar com Rows.com.")
            return

        logger.info(f"Encontrados {len(unsynced_data)} registros para sincronizar.")

        # 2. TransformaÃ§Ã£o (Transform)
        # Prepara a matriz de valores para o RowsAppendRequest
        values_matrix = []
        bueiros_to_mark = []
        
        for record in unsynced_data:
            linha = [
                str(record.id_bueiro), 
                record.distancia_cm, 
                record.nivel_obstrucao,
                record.status, 
                record.latitude, 
                record.longitude, 
                record.ultima_atualizacao.isoformat()
            ]
            values_matrix.append(linha)
            # Acumulamos o ID para atualizar o status no banco apÃ³s o envio
            bueiros_to_mark.append(str(record.id_bueiro))
            
        payload = RowsAppendRequest(values=values_matrix)
        
        # 3. Carga (Load)
        try:
            success = await self.rows_service.append_data(
                spreadsheet_id=self.spreadsheet_id,
                table_id=self.table_id,
                payload=payload
            )
            
            if success:
                # Marca como sincronizado no banco de dados
                await self.repository.mark_as_synced(bueiros_to_mark)
                logger.info(f"SincronizaÃ§Ã£o concluÃda com sucesso. Registros marcados como sincronizados no DB.")
        
        except Exception as e:
            logger.error(f"Falha na rotina de sincronizaÃ§Ã£o com o Rows.com: {e}")
