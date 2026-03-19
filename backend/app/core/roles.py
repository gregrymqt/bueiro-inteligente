from fastapi import Depends, HTTPException, status
from backend.app.core.security import get_current_user
from backend.app.features.auth.dto import UserTokenData

class RoleChecker:
    """
    Dependência que verifica se o usuário logado possui uma das roles permitidas.
    """
    def __init__(self, allowed_roles: list[str]):
        self.allowed_roles = allowed_roles

    def __call__(self, user: UserTokenData = Depends(get_current_user)):
        if not any(role in user.roles for role in self.allowed_roles):
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail="Você não tem permissão para acessar este recurso."
            )
        return user