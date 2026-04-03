from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
from sqlalchemy.orm import joinedload
from .interfaces import IAuthRepository
from .dto import UserInDB
from .models import User, Role
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
            stmt = select(User).options(joinedload(User.role)).where(User.email == email)
            result = await self.db.execute(stmt)
            user_record = result.scalars().first()

            if not user_record:
                logger.debug(f"Usuário não encontrado no banco de dados: {email}")
                return None

            return UserInDB(
                email=user_record.email,
                full_name=user_record.full_name,
                hashed_password=user_record.hashed_password,
                role=user_record.role.name if user_record.role else "User"
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
            
            # Precisamos buscar o ID da Role correspondente
            role_stmt = select(Role).where(Role.name == user_in_db.role)
            result = await self.db.execute(role_stmt)
            role_record = result.scalars().first()
            
            db_user = User(
                email=user_in_db.email,
                full_name=user_in_db.full_name,
                hashed_password=user_in_db.hashed_password,
                role_id=role_record.id if role_record else 3 # ID 3 para 'User' fallback
            )

            self.db.add(db_user)
            await self.db.commit()
            
            # Para pegar o user recém persistido com sua Role (se precisarmos logo em seguida)
            stmt = select(User).options(joinedload(User.role)).where(User.id == db_user.id)
            result = await self.db.execute(stmt)
            fresh_user = result.scalars().first()

            if not fresh_user:
                raise ValueError("Usuário não foi persistido corretamente no banco de dados.")

            logger.info(f"Usuário salvo com sucesso no banco de dados: {user_in_db.email}")
            return UserInDB(
                email=fresh_user.email,
                full_name=fresh_user.full_name,
                hashed_password=fresh_user.hashed_password,
                role=fresh_user.role.name if fresh_user.role else "User"
            )
        except Exception as e:
            await self.db.rollback()
            logger.error(f"Erro ao salvar usuário no banco de dados ({user_in_db.email}): {str(e)}", exc_info=True)
            raise
