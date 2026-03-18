from fastapi import Depends, HTTPException, status
from backend.app.core.security import get_current_user
from backend.app.features.auth.dto import User

# Primeiro, precisamos de uma função que extraia o usuário completo do token
async def get_current_user_data(payload: dict = Depends(get_current_user)) -> User:
    username = payload.get("sub")
    roles = payload.get("roles", []) # Extrai as roles direto do token!
    
    # Montamos o objeto User sem precisar ir ao banco de dados!
    return User(username=username, roles=roles)

# O Guarda-costas
class RoleChecker:
    """
    Guarda-costas das rotas. Verifica se o usuário tem a permissão necessária.
    """
    def __init__(self, allowed_roles: list[str]):
        self.allowed_roles = allowed_roles

    def __call__(self, user: dict = Depends(get_current_user_data)):
        # Verifica se alguma das roles do usuário bate com as permitidas na rota
        for role in user.get("roles", []):
            if role in self.allowed_roles:
                return user # Acesso liberado!
        
        # Se não encontrou a role, bloqueia a requisição
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Operação não permitida. Você não tem o perfil de acesso necessário."
        )