from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    # Configurações Gerais
    PROJECT_NAME: str
    VERSION: str
    API_STR: str

    # JWT
    SECRET_KEY: str
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 30

    # Adafruit
    ADAFRUIT_IO_USERNAME: str
    ADAFRUIT_IO_KEY: str
    ADAFRUIT_WEBHOOK_TOKEN: str

    # Supabase
    SUPABASE_URL: str
    SUPABASE_KEY: str

    # Redis
    REDIS_HOST: str = "localhost"
    REDIS_PORT: int = 6379
    REDIS_DB: int = 0
    REDIS_PASSWORD: str | None = None

    model_config = SettingsConfigDict(env_file=".env")

settings = Settings()