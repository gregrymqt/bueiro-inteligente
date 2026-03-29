# app/extensions/scheduler.py
import logging
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from app.core.config import settings

# ImportaÃ§Ãµes dos componentes que vocÃª enviou no logError.md
from app.features.rows.services import RowsService
from app.features.rows.job import RowsSyncJob
from app.features.monitoring.repository import DrainRepository
from app.features.cache.service import RedisCacheService

# Importamos a infraestrutura para pegar os clientes de DB e Cache
from app.extensions.infrastructure import infrastructure

logger = logging.getLogger(__name__)

class SchedulerExtension:
    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(SchedulerExtension, cls).__new__(cls)
            cls._instance.scheduler = AsyncIOScheduler()
            cls._instance.rows_service = None
        return cls._instance

    async def open(self):
        """
        Inicializa o RowsService e agenda o Job de sincronizaÃ§Ã£o.
        Equivalente ao setup_scheduler que vocÃª tinha, mas dentro da extensÃ£o.
        """
        logger.info("Iniciando Agendador de Tarefas (APScheduler)...")

        try:
            # 1. Instancia o Service do Rows com a API Key das configuraÃ§Ãµes
            self.rows_service = RowsService(api_key=settings.ROWS_API_KEY)      

            # 2. Instancia o Repository (NecessÃ¡rio para o Job ler os dados do Supabase)
            # Usamos o cliente que jÃ¡ foi aberto na InfrastructureExtension  
            cache_service = RedisCacheService(infrastructure.redis_client)      
            repository = DrainRepository(
                db_client=infrastructure.supabase,
                cache_service=cache_service
            )

            # 3. Configura a instÃ¢ncia do Job
            sync_job = RowsSyncJob(
                repository=repository,
                rows_service=self.rows_service,
                spreadsheet_id=settings.ROWS_SPREADSHEET_ID,
                table_id=settings.ROWS_TABLE_ID
            )

            # Wrapper para garantir execuÃ§Ã£o Ãºnica via Redis Lock
            async def execute_with_lock():
                redis_client = infrastructure.redis_client
                # Tenta adquirir o lock por 300 segundos (5 min). NX garante que sÃ³ adquire se nÃ£o existir
                # Dessa forma, mÃºltiplos workers do Uvicorn nÃ£o rodam o Job silmuntaneamente
                lock_acquired = await redis_client.set("lock:rows_sync_job", "locked", nx=True, ex=300)
                if not lock_acquired:
                    logger.debug("RowsSyncJob bypassado: job jÃ¡ rodando em outro worker Uvicorn.")
                    return
                
                try:
                    logger.info("Lock adquirido com sucesso. Executando sincronizaÃ§Ã£o Rows...")
                    await sync_job.execute()
                except Exception as e:
                    logger.error(f"Erro durante execuÃ§Ã£o do Job de SincronizaÃ§Ã£o (Worker): {e}")

            # 4. Adiciona o Job ao agendador (Rodando a cada 60 minutos)
            self.scheduler.add_job(
                execute_with_lock,
                'interval',
                minutes=60,
                id='sync_rows_job',
                replace_existing=True
            )

            # 5. Liga o motor do agendador
            self.scheduler.start()
            logger.info("Scheduler iniciado: Job do Rows agendado com sucesso e protegido por redis-lock.")

        except Exception as e:
            logger.error(f"Erro ao iniciar o Scheduler: {e}")

    async def close(self):
        """Desliga o agendador graciosamente no shutdown da API."""
        if self.scheduler.running:
            logger.info("Encerrando Agendador de Tarefas...")
            self.scheduler.shutdown()
            logger.info("Scheduler encerrado.")

# Singleton para exportaÃ§Ã£o
scheduler_extension = SchedulerExtension()
