from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
from .interfaces import IAuthRepository
from .dto import UserInDB
from .models import User
import logging

logger = logging.getLogger(__name__)

class AuthRepository(IAuthRepository):
    """
    Repositório de autenticação que se comunica com o banco usando SQLAlchemy assíncrono.
    """
    def __init__(self, db: AsyncSession):
        self.db = db

    async def get_user_by_email(self, email: str) -> UserInDB | None:
        """
        Busca um usuário no banco de dados pelo seu email.
        """
        try:
            logger.debug(f"Buscando usuário por email no banco de dados: {email}")
            stmt = select(User).where(User.email == email)
            result = await self.db.execute(stmt)
            user_record = result.scalars().first()

            if not user_record:
                logger.debug(f"Usuário não encontrado no banco de dados: {email}")
                return None

            return UserInDB(
                email=user_record.email,
                full_name=user_record.full_name,
                hashed_password=user_record.hashed_password,
                roles=user_record.roles
            )
        except Exception as e:
            logger.error(f"Erro no banco de dados ao buscar usuário ({email}): {str(e)}", exc_info=True)
            raise

    async def create_user(self, user_in_db: UserInDB) -> UserInDB:
        """
        Salva um novo usuário no banco de dados.
        """
        try:
            logger.info(f"Persistindo novo usuário no banco de dados: {user_in_db.email}")
            db_user = User(
                email=user_in_db.email,
                full_name=user_in_db.full_name,
                hashed_password=user_in_db.hashed_password,
                roles=user_in_db.roles
            )

            self.db.add(db_user)
            await self.db.commit()
            await self.db.refresh(db_user)

            logger.info(f"Usuário salvo com sucesso no banco de dados: {user_in_db.email}")
            return UserInDB(
                email=db_user.email,
                full_name=db_user.full_name,
                hashed_password=db_user.hashed_password,
                roles=db_user.roles
            )
        except Exception as e:
            logger.error(f"Erro no banco de dados ao tentar criar usuário ({user_in_db.email}): {str(e)}", exc_info=True)
            await self.db.rollback()
            raise
