from contextlib import asynccontextmanager
from backend.app.features.rows.services import RowsService
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import os

# Importando o roteador que criamos no controller
from app.features.monitoring import controller as monitoring_controller
from app.features.auth.repository import mock_auth_repo
from app.features.auth import controller as auth_controller # Importa o controller de autenticação
from app.features.monitoring.controller import router as monitoring_router
from app.features.realtime.controller import router as realtime_router


# Importações do nosso novo Job e das interfaces/implementações
from app.core.scheduler import setup_scheduler, scheduler
from app.features.monitoring.repository import DrainRepository

# NOTA: Importa as tuas instâncias reais de base de dados e cache aqui
from app.core.database import get_db
from app.core.cache import get_cache


@asynccontextmanager
async def lifespan(app: FastAPI):
    # =======================================================
    # 1. SETUP INICIAL (Executa quando a API arranca)
    # =======================================================
    print("A iniciar ligações e serviços em background...")

    # Inicializa o repositório de autenticação mock
    await mock_auth_repo.initialize()
    
    # Instanciamos as nossas dependências (Inversão de Dependência)
    repo = DrainRepository(get_db(), get_cache())
    rows_service = RowsService(api_key=os.getenv("ROWS_API_KEY"))
    
    # Configuramos o scheduler com os dados do ambiente
    setup_scheduler(
        repository=repo, 
        rows_service=rows_service, 
        spreadsheet_id=os.getenv("ROWS_SPREADSHEET_ID"), 
        table_id=os.getenv("ROWS_TABLE_ID")
     )
    
    # Arrancamos o job! Ele vai ficar a correr na sua própria thread/task
    scheduler.start()
    print("Scheduler iniciado com sucesso. Job de sincronização com o Rows ativo.")

    yield # <--- Aqui o FastAPI assume o controlo e começa a receber os pedidos HTTP

    # =======================================================
    # 2. SHUTDOWN (Executa quando paras a API / matas o contentor)
    # =======================================================
    print("A encerrar a aplicação... A parar os jobs em background.")
    scheduler.shutdown()


# 1. Instanciando o servidor FastAPI (agora com o lifespan injetado)
app = FastAPI(
    title=os.getenv("PROJECT_NAME"),
    description="Backend de monitoramento IoT, ETL e Cache",
    version=os.getenv("VERSION"),
    lifespan=lifespan # <--- Conectamos o ciclo de vida aqui
)

# 2. Configurando o CORS (Crucial para o React)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  
    allow_credentials=True,
    allow_methods=["*"], 
    allow_headers=["*"],
)


# 3. Conectando as rotas da nossa feature
app.include_router(monitoring_controller.router)
app.include_router(auth_controller.router) # Adiciona as rotas de autenticação
app.include_router(realtime_router)

# 4. Rota de Health Check (Teste rápido)
@app.get("/", tags=["Sistema"])
async def root():
    return {
        "status": "online", 
        "projeto": os.getenv("PROJECT_NAME"),
        "tecnologias": "FastAPI, Redis, Supabase"
    }
