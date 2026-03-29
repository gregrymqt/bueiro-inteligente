import { apiClient } from '@/core/http/ApiClient';
import type { LoginRequestDTO, LoginResponseDTO, UserDTO, RegisterRequestDTO } from '../types';     

export class AuthService {
  /**
   * Realiza o cadastro de um novo usuário.
   */
  public static async register(data: RegisterRequestDTO): Promise<UserDTO> {
    return apiClient.post<UserDTO, RegisterRequestDTO>('/auth/register', data);
  }

  /**
   * Realiza o login e obtém o token.
   */
  public static async login(credentials: LoginRequestDTO): Promise<LoginResponseDTO> {
    return apiClient.post<LoginResponseDTO, LoginRequestDTO>('/auth/login', credentials);
  }

  /**
   * Invalida o token atual no servidor (Blacklist via Redis).
   */
  public static async logout(): Promise<void> {
    // O interceptor injeta o token automaticamente no Header
    return apiClient.post<void, void>('/auth/logout');
  }

  /**
   * Obtém os dados do usuário logado (Perfil e Roles).
   */
  public static async getMe(): Promise<UserDTO> {
    return apiClient.get<UserDTO>('/auth/users/me');
  }
}