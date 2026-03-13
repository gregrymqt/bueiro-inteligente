from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

# Importando o roteador que criamos no controller
from app.features.monitoring import controller as monitoring_controller

# 1. Instanciando o servidor FastAPI
app = FastAPI(
    title="API - Bueiro Inteligente",
    description="Backend de monitoramento IoT, ETL e Cache",
    version="1.0.0"
)

# 2. Configurando o CORS (Crucial para o React)
# Sem isso, o navegador bloqueia o seu Front-end de conversar com o Python
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Em produção, você colocaria a URL exata do seu React
    allow_credentials=True,
    allow_methods=["*"],  # Permite GET, POST, PUT, DELETE
    allow_headers=["*"],
)

# 3. Conectando as rotas da nossa feature
# É aqui que o main.py "descobre" que existe um Webhook e um GET
app.include_router(monitoring_controller.router)

# 4. Rota de Health Check (Teste rápido)
@app.get("/", tags=["Sistema"])
async def root():
    return {
        "status": "online", 
        "projeto": "Bueiro Inteligente",
        "tecnologias": "FastAPI, Redis, Supabase"
    }