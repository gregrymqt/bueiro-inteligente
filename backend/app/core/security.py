import logging
from fastapi import Request, HTTPException, status, WebSocket, WebSocketException
from app.extensions.infrastructure import InfrastructureExtension

logger = logging.getLogger(__name__)

class RateLimiter:
    """
    Sistema de Rate-Limiting Assíncrono para o FastAPI consumindo Redis.
    """
    def __init__(self, times: int = 5, seconds: int = 10):
        self.times = times
        self.seconds = seconds

    async def __call__(self, request: Request):
        # Obtém o IP do cliente (Suporte a Proxy/Render via X-Forwarded-For). 
        forwarded = request.headers.get("x-forwarded-for")
        if forwarded:
            client_ip = forwarded.split(",")[0].strip()
        else:
            client_ip = request.client.host if request.client else "127.0.0.1"
        
        # Caso o sistema de autenticação injete o user no request.state, usamos ele priorizando o IP.
        user_id = getattr(request.state, "user_id", getattr(request.state, "user", None))
        identifier = str(user_id) if user_id else client_ip
        
        route_path = request.url.path
        redis_key = f"rate_limit:{identifier}:{route_path}"
        
        infra = InfrastructureExtension()
        redis_client = infra.redis_client
        
        # Se o Redis não estiver pronto, apenas permite (evita travar rotas críticas por falha no cache)
        if not redis_client:
            logger.warning("RateLimiter: Cliente Redis não inicializado. Ignorando rate limit.")
            return

        try:
            # Pega o contador atual
            current_count = await redis_client.get(redis_key)
            
            if current_count and int(current_count) >= self.times:
                raise HTTPException(
                    status_code=status.HTTP_429_TOO_MANY_REQUESTS,
                    detail="Too Many Requests. Limite de requisições excedido."
                )
            
            # Incrementa atômicamente no pipeline, aplicando o TTL apenas na primeira requisição
            await redis_client.incr(redis_key)
            if not current_count:
                await redis_client.expire(redis_key, self.seconds)
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Erro no validador do Redis (RateLimiter): {e}")

class WebSocketRateLimiter:
    """
    Sistema de Rate-Limiting para Conexões WebSockets usando Redis.
    """
    def __init__(self, times: int = 5, seconds: int = 10):
        self.times = times
        self.seconds = seconds

    async def __call__(self, websocket: WebSocket):
        # Obtém o IP do cliente (Suporte a Proxy/Render via X-Forwarded-For). 
        forwarded = websocket.headers.get("x-forwarded-for")
        if forwarded:
            client_ip = forwarded.split(",")[0].strip()
        else:
            client_ip = websocket.client.host if websocket.client else "127.0.0.1"
        
        # Assim como nas requests HTTP, tentamos buscar o user no state interligado
        user_id = getattr(websocket.state, "user_id", getattr(websocket.state, "user", None))
        identifier = str(user_id) if user_id else client_ip
        
        route_path = websocket.url.path
        redis_key = f"ws_rate_limit:{identifier}:{route_path}"
        
        infra = InfrastructureExtension()
        redis_client = infra.redis_client
        
        if not redis_client:
            logger.warning("WebSocketRateLimiter: Cliente Redis não inicializado.")
            return

        try:
            current_count = await redis_client.get(redis_key)
            
            if current_count and int(current_count) >= self.times:
                logger.warning(f"WebSocket recusado por Rate Limit. IP: {client_ip}")
                # 1008 = Policy Violation
                raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION, reason="Rate Limit Exceeded")
            
            await redis_client.incr(redis_key)
            if not current_count:
                await redis_client.expire(redis_key, self.seconds)
            
        except WebSocketException:
            raise
        except Exception as e:
            logger.error(f"Erro no validador do Redis (WebSocketRateLimiter): {e}")
