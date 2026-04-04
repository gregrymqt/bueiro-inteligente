from fastapi import APIRouter, Depends, HTTPException, status
from .dto import Token, User, LoginRequest, UserTokenData, UserCreate
from .service import AuthService, get_auth_service
from app.extensions.auth import RoleChecker
from app.core.security import RateLimiter
import logging

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/auth", tags=["Autenticação"])

# =======================================================
# Endpoints de Autenticação
# =======================================================
@router.post("/login", response_model=Token, summary="Obter Token de Acesso", dependencies=[Depends(RateLimiter(times=5, seconds=10))])   
async def login_for_access_token(
    credentials: LoginRequest,
    auth_service: AuthService = Depends(get_auth_service)
):
    """
    Autentica o usuário e retorna um token de acesso.
    """
    try:
        logger.info(f"Requisição de login recebida para: {credentials.email}")
        user = await auth_service.authenticate_user(credentials.email, credentials.password)
        if not user:
            logger.warning(f"Falha de autenticação via controller para: {credentials.email}")
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Incorrect email or password",
                headers={"WWW-Authenticate": "Bearer"},
            )
        access_token = auth_service.create_access_token(user)
        logger.info(f"Retornando token de acesso para: {credentials.email}")
        return {"access_token": access_token, "token_type": "bearer"}
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Erro inesperado no controller de login para {credentials.email}: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro interno no servidor")


@router.post("/register", response_model=User, status_code=status.HTTP_201_CREATED, summary="Cadastrar novo usuário", dependencies=[Depends(RateLimiter(times=5, seconds=60))])
async def register(
    user_in: UserCreate,
    auth_service: AuthService = Depends(get_auth_service)
):
    """
    Cadastra um novo usuário no sistema. Regras de negócio cuidam do hash da senha e validação de duplicidade.
    """
    try:
        logger.info(f"Requisição de registro recebida para: {user_in.email}")
        result = await auth_service.register_user(user_in)
        logger.info(f"Registro finalizado com sucesso via controller para: {user_in.email}")
        return result
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Erro inesperado no controller de registro para {user_in.email}: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro interno no servidor")


@router.post("/logout", summary="Revogar Token (Logout)", dependencies=[Depends(RateLimiter(times=5, seconds=10))])
async def logout(
    current_user: UserTokenData = Depends(RoleChecker(['Admin', 'Manager', 'User'])),
    auth_service: AuthService = Depends(get_auth_service)
):
    """
    Adiciona o token atual à blacklist para invalidá-lo.
    """
    try:
        logger.info(f"Requisição de logout recebida de: {current_user.email}")
        await auth_service.logout(current_user.jti)
        logger.info(f"Logout completado com sucesso via controller para: {current_user.email}")
        return {"message": "Logout successful"}
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Erro inesperado no controller de logout para {current_user.email}: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro interno no servidor")


@router.get("/users/me", response_model=User, summary="Obter informações do usuário atual", dependencies=[Depends(RateLimiter(times=10, seconds=10))])
async def read_users_me(
    current_user: UserTokenData = Depends(RoleChecker(['Admin', 'Manager', 'User'])),
    auth_service: AuthService = Depends(get_auth_service)
):
    """
    Rota protegida que retorna as informações do usuário logado consultando o banco.
    """
    try:
        logger.info(f"Requisição de profile (users/me) recebida de: {current_user.email}")
        user_in_db = await auth_service.repository.get_user_by_email(current_user.email)

        if user_in_db:
            return User(
                email=user_in_db.email,
                full_name=user_in_db.full_name,
                roles=user_in_db.roles
            )

        logger.warning(f"Usuário não encontrado na base para o token: {current_user.email}")
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="User not found")
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Erro inesperado no controller de usuario profile para {current_user.email}: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro interno no servidor")
