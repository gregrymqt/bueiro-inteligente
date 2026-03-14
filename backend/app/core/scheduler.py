import logging
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from backend.app.features.rows.interfaces import IRowsService
from app.features.rows.dtos import RowsAppendRequest
from app.features.monitoring.interfaces import IDrainRepository

logger = logging.getLogger(__name__)

class RowsSyncJob:
    def __init__(self, repository: IDrainRepository, rows_service: IRowsService, spreadsheet_id: str, table_id: str):
        self.repository = repository
        self.rows_service = rows_service
        self.spreadsheet_id = spreadsheet_id
        self.table_id = table_id

    async def execute(self):
        """Método que será executado periodicamente pelo scheduler."""
        logger.info("Iniciando Job de sincronização com o Rows...")
        
        # 1. Busca até 100 registros não sincronizados
        unsynced_records = await self.repository.get_unsynced_data(limit=100)
        
        if not unsynced_records:
            logger.info("Nenhum dado novo para sincronizar.")
            return

        # 2. Formata os dados para o DTO do Rows (Lista de Listas)
        # Ajuste a ordem das colunas conforme sua tabela no Rows
        values_to_append = []
        ids_to_mark = []
        
        for record in unsynced_records:
            values_to_append.append([
                record.id_bueiro,
                record.distancia_cm,
                record.nivel_obstrucao,
                record.status,
                record.ultima_atualizacao.isoformat()
            ])
            ids_to_mark.append(record.id_bueiro) # Pegando o ID para marcar depois

        payload = RowsAppendRequest(values=values_to_append)

        # 3. Envia para a Service do Rows usando try-except para segurança
        try:
            sucesso = await self.rows_service.append_data(
                spreadsheet_id=self.spreadsheet_id,
                table_id=self.table_id,
                payload=payload
            )
            
            # 4. Se deu sucesso, marca no Supabase para não enviar de novo
            if sucesso:
                await self.repository.mark_as_synced(ids_to_mark)
                logger.info(f"Sucesso! {len(ids_to_mark)} registros sincronizados.")
                
        except Exception as e:
            logger.error(f"Falha na execução do Job do Rows: {e}")

# Instância global do Scheduler
scheduler = AsyncIOScheduler()

def setup_scheduler(repository: IDrainRepository, rows_service: IRowsService, spreadsheet_id: str, table_id: str):
    """Configura e adiciona os jobs ao scheduler."""
    job_instance = RowsSyncJob(repository, rows_service, spreadsheet_id, table_id)
    
    # Configura para rodar a cada 60 minutos (ajuste conforme a necessidade)
    scheduler.add_job(job_instance.execute, 'interval', minutes=60, id='sync_rows_job')
    
    return scheduler