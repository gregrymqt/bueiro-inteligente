from fastapi import APIRouter, Depends, HTTPException, status
from .dto import Token, User, LoginRequest, UserTokenData
from .service import AuthService
from .repository import mock_auth_repo
from backend.app.core.security import get_current_user

router = APIRouter(prefix="/auth", tags=["Autenticação"])


# =======================================================
# Injeção de Dependência (Simples)
# =======================================================
# Em um projeto maior, você usaria um container de injeção de dependência
# como o `fastapi-injector` ou o `dependency-injector`.
# Aqui, vamos instanciar diretamente para manter a simplicidade.
auth_service = AuthService(repository=mock_auth_repo)

# =======================================================
# Endpoints de Autenticação
# =======================================================
@router.post("/login", response_model=Token, summary="Obter Token de Acesso")
async def login_for_access_token(credentials: LoginRequest):
    """
    Autentica o usuário e retorna um token de acesso.
    """
    user = await auth_service.authenticate_user(credentials.email, credentials.password)
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect username or password",
            headers={"WWW-Authenticate": "Bearer"},
        )
    access_token = auth_service.create_access_token(user)
    return {"access_token": access_token, "token_type": "bearer"}


@router.post("/logout", summary="Revogar Token (Logout)")
async def logout(current_user: UserTokenData = Depends(get_current_user)):
    """
    Adiciona o token atual à blacklist para invalidá-lo.
    """
    await auth_service.logout(current_user.jti)
    return {"message": "Logout successful"}


@router.get("/users/me", response_model=User, summary="Obter informações do usuário atual")
async def read_users_me(current_user: UserTokenData = Depends(get_current_user)):
    """
    Rota protegida que retorna as informações do usuário logado.
    """
    # O `get_current_user` já valida o token. Aqui, apenas retornamos os dados.
    # Em um caso real, você poderia buscar dados mais detalhados do usuário no banco.
    user_in_db = await mock_auth_repo.get_user_by_username(current_user.username)
    if user_in_db:
        return User(username=user_in_db.username, full_name=user_in_db.full_name, roles=user_in_db.roles)
    
    # Isso não deve acontecer se o token for válido, mas é uma boa prática de segurança
    raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="User not found")
