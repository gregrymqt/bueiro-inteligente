from fastapi import APIRouter, Depends, HTTPException, status
from .dto import Token, User, LoginRequest, UserTokenData
from .service import AuthService
from .repository import mock_auth_repo
from app.extensions.auth import get_current_user

router = APIRouter(prefix="/auth", tags=["AutenticaÃ§Ã£o"])


# =======================================================
# InjeÃ§Ã£o de DependÃªncia (Simples)
# =======================================================
# Em um projeto maior, vocÃª usaria um container de injeÃ§Ã£o de dependÃªncia
# como o `fastapi-injector` ou o `dependency-injector`.
# Aqui, vamos instanciar diretamente para manter a simplicidade.
auth_service = AuthService(repository=mock_auth_repo)

# =======================================================
# Endpoints de AutenticaÃ§Ã£o
# =======================================================
@router.post("/login", response_model=Token, summary="Obter Token de Acesso")
async def login_for_access_token(credentials: LoginRequest):
    """
    Autentica o usuÃ¡rio e retorna um token de acesso.
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
    Adiciona o token atual Ã  blacklist para invalidÃ¡-lo.
    """
    await auth_service.logout(current_user.jti)
    return {"message": "Logout successful"}


@router.get("/users/me", response_model=User, summary="Obter informaÃ§Ãµes do usuÃ¡rio atual")
async def read_users_me(current_user: UserTokenData = Depends(get_current_user)):
    """
    Rota protegida que retorna as informaÃ§Ãµes do usuÃ¡rio logado.
    """
    # O `get_current_user` jÃ¡ valida o token. Aqui, apenas retornamos os dados.
    # Em um caso real, vocÃª poderia buscar dados mais detalhados do usuÃ¡rio no banco.
    user_in_db = await mock_auth_repo.get_user_by_username(current_user.username)
    if user_in_db:
        return User(username=user_in_db.username, full_name=user_in_db.full_name, roles=user_in_db.roles)
    
    # Isso nÃ£o deve acontecer se o token for vÃ¡lido, mas Ã© uma boa prÃ¡tica de seguranÃ§a
    raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="User not found")
