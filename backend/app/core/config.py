# app/core/config.py
from pydantic_settings import BaseSettings, SettingsConfigDict
from pydantic import field_validator
from typing import Any
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
    REDIS_URL: str = ""
    REDIS_EXTERNAL_URL: str = "" 
    REDIS_LOCAL: bool = False
    REDIS_SSL: bool = False
    
    # Configurações do Supabase (PostgreSQL)
    DATABASE_URL: str = ""
    MIGRATIONS_URL: str = "" # URL específica para as migrações (pode ser a mesma do DATABASE_URL ou diferente)

    # ROWS (Planilhas / Scheduler)
    # Adicionado para o SchedulerExtension conseguir sincronizar os dados
    ROWS_API_KEY: str = ""
    ROWS_SPREADSHEET_ID: str = ""
    ROWS_TABLE_ID: str = ""

    # CORS
    ALLOWED_ORIGINS: Any = ["*"]

    @field_validator("ALLOWED_ORIGINS", mode="before")
    @classmethod
    def assemble_cors_origins(cls, v: Any) -> list[str]:
        if isinstance(v, str) and not v.startswith("["):
            return [i.strip() for i in v.split(",") if i.strip()]
        elif isinstance(v, list):
            return v
        return ["*"]

    # Carrega do arquivo .env absoluto com base na pasta backend
    model_config = SettingsConfigDict(env_file=str(BASE_DIR / ".env"), extra="ignore")

settings = Settings()