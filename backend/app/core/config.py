# app/core/config.py
from pydantic_settings import BaseSettings, SettingsConfigDict
from pathlib import Path

# Define a raiz da pasta backend (3 níveis acima do config.py)
BASE_DIR = Path(__file__).resolve().parent.parent.parent

class Settings(BaseSettings):
    # Configurações Gerais
    PROJECT_NAME: str = "Bueiro Inteligente"
    VERSION: str = "1.0.0"
    API_STR: str = "/api/v1"

    # JWT / Segurança
    SECRET_KEY: str = ""
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 30
    
    # TOKEN DO HARDWARE (ESP32 / IoT)
    # Adicionado para validar as requisições que vêm da placa
    HARDWARE_TOKEN: str = ""

    # Redis
    REDIS_HOST: str = "localhost"
    REDIS_PORT: int = 6379
    REDIS_DB: int = 0
    REDIS_PASSWORD: str | None = None
    
    # Configurações do Supabase (PostgreSQL)
    DATABASE_URL: str = ""
    MIGRATIONS_URL: str = "" # URL específica para as migrações (pode ser a mesma do DATABASE_URL ou diferente)

    # ROWS (Planilhas / Scheduler)
    # Adicionado para o SchedulerExtension conseguir sincronizar os dados
    ROWS_API_KEY: str = ""
    ROWS_SPREADSHEET_ID: str = ""
    ROWS_TABLE_ID: str = ""

    # Carrega do arquivo .env absoluto com base na pasta backend
    model_config = SettingsConfigDict(env_file=str(BASE_DIR / ".env"), extra="ignore")

settings = Settings()