from pydantic import BaseModel, EmailStr

# =======================================================
# DTOs para Login e Tokens
# =======================================================

class LoginRequest(BaseModel):
    email: EmailStr
    password: str

class Token(BaseModel):
    """
    DTO para a resposta do endpoint de login, contendo o token e o tipo.
    """
    access_token: str
    token_type: str

class TokenPayload(BaseModel):
    """
    DTO para o conteúdo (payload) do token JWT.
    """
    sub: str | None = None # Subject (geralmente o username ou ID do usuário)
    jti: str | None = None # JWT ID (identificador único do token)


# =======================================================
# DTOs para Usuários
# =======================================================

class UserBase(BaseModel):
    """
    DTO base com os campos comuns de um usuário.
    """
    username: EmailStr
    full_name: str | None = None

class User(UserBase):
    """
    DTO para retornar informações seguras de um usuário (sem a senha).
    """
    pass

class UserInDB(UserBase):
    """
    DTO que representa o usuário como ele está no "banco de dados",
    incluindo o hash da senha.
    """
    hashed_password: str
