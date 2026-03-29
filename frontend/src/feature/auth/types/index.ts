export interface UserDTO {
  email: string;
  full_name: string | null;
  roles: string[]; // ['admin', 'manutencao', 'cidadao']
}

export interface RegisterRequestDTO {
  email: string;
  password: string;
  full_name?: string;
}

export interface LoginRequestDTO {
  email: string;
  password: string;
}

export interface LoginResponseDTO {
  access_token: string;
  token_type: string;
}