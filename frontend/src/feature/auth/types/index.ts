// O que enviamos para o FastAPI
export interface LoginRequestDTO {
  email: string;
  password: string;
}

// O que o FastAPI nos devolve
export interface LoginResponseDTO {
  access_token: string;
  token_type: string;
}