from supabase import create_async_client, AsyncClient
from .config import settings

# Inicializa o cliente assíncrono do Supabase
supabase: AsyncClient = create_async_client(settings.SUPABASE_URL, settings.SUPABASE_KEY)

async def get_db() -> AsyncClient:
    """
    Retorna o cliente de banco de dados assíncrono.
    """
    return supabase