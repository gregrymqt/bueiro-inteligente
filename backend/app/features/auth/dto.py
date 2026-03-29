from pydantic import BaseModel, EmailStr, Field

# =======================================================
# DTOs para Login e Tokens
# =======================================================

class LoginRequest(BaseModel):
    email: str
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
    sub: str | None = None # Subject (geralmente o email ou ID do usuÃ¡rio)  
    jti: str | None = None
    roles: list[str] = Field(default_factory=list) # Roles do usuario


# =======================================================
# DTOs para Usuários
# =======================================================

class UserBase(BaseModel):
    email: EmailStr
    full_name: str | None = None
    roles: list[str] = Field(default_factory=list)

class UserCreate(BaseModel):
    """
    DTO para payload de cadastro/registro de um novo usuário.
    """
    email: EmailStr
    password: str = Field(..., min_length=6, description="A senha deve ter no mínimo 6 caracteres")
    full_name: str | None = None
    roles: list[str] = Field(default_factory=list)

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

class UserTokenData(BaseModel):
    email: str
    roles: list[str] = []
    jti: str
