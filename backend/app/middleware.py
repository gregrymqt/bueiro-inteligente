from fastapi import Request, HTTPException, status
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.responses import Response
from jose import jwt, JWTError
from backend.app.core.config import settings
from backend.app.core.blacklist import is_blacklisted

class JWTMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        # Rotas que não precisam de autenticação
        public_paths = ["/docs", "/openapi.json", "/token", "/"]
        
        # Se o caminho do pedido estiver nas rotas públicas, segue em frente
        if request.url.path in public_paths:
            response = await call_next(request)
            return response

        # Pega o header de autorização
        auth_header = request.headers.get("Authorization")
        if not auth_header or not auth_header.startswith("Bearer "):
            return Response("Not authenticated", status_code=status.HTTP_401_UNAUTHORIZED)

        # Extrai o token
        token = auth_header.split(" ")[1]

        try:
            # Decodifica o payload do token
            payload = jwt.decode(token, settings.SECRET_KEY, algorithms=[settings.ALGORITHM])
            jti = payload.get("jti")

            # Verifica se o JTI (identificador do token) existe
            if jti is None:
                return Response("Invalid token", status_code=status.HTTP_401_UNAUTHORIZED)

            # Verifica se o token está na blacklist
            if await is_blacklisted(jti):
                return Response("Token has been revoked", status_code=status.HTTP_401_UNAUTHORIZED)

        except JWTError:
            # Se houver erro na decodificação (token inválido, expirado, etc.)
            return Response("Invalid token", status_code=status.HTTP_401_UNAUTHORIZED)
        
        # Se tudo estiver OK, prossegue para o endpoint
        response = await call_next(request)
        return response
