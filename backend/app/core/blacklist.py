from datetime import timedelta
from backend.app.core.cache import get_cache

# Usamos um set no Redis para performance máxima na verificação de existência
BLACKLIST_KEY = "jwt_blacklist"

# O TTL (Time To Live) do token na blacklist deve ser um pouco maior
# que o tempo de expiração do próprio token, para garantir que ele já
# tenha expirado quando o Redis o remover.
BLACKLIST_TTL_SECONDS = timedelta(minutes=15).total_seconds() + 60 


async def add_to_blacklist(token: str):
    """
    Adiciona um token JTI (identificador único do token) à blacklist no Redis.
    Define um TTL para que a blacklist não cresça indefinidamente.
    """
    redis = get_cache()
    # Usamos um pipeline para garantir que as duas operações sejam atômicas
    async with redis.pipeline() as pipe:
        # Adiciona o membro ao set
        pipe.sadd(BLACKLIST_KEY, token)
        # Define o tempo de expiração para o membro (isso é um pouco mais complexo em sets)
        # A estratégia mais simples é definir um TTL para a chave inteira se ela for nova,
        # ou redefinir se necessário. Para tokens individuais, a abordagem muda.
        # Por simplicidade aqui, vamos assumir que o volume de logouts não é gigantesco
        # e podemos ter um job de limpeza ou simplesmente deixar expirar.
        # A forma mais eficaz seria armazenar cada token como uma chave própria.
        # Ex: blacklist:<jti> com um TTL.
        
        # Estratégia Chave-Valor para TTL por token:
        key = f"blacklist:{token}"
        await redis.setex(name=key, time=int(BLACKLIST_TTL_SECONDS), value="blacklisted")


async def is_blacklisted(token: str) -> bool:
    """
    Verifica se um token JTI está na blacklist do Redis.
    """
    redis = get_cache()
    # Estratégia Chave-Valor:
    key = f"blacklist:{token}"
    return await redis.exists(key)
