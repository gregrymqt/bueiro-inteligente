import { apiClient } from '@/core/http/ApiClient';
import { isMockDataSourceEnabled } from '@/core/http/environment';
import type { LoginRequestDTO, LoginResponseDTO, UserDTO, RegisterRequestDTO } from '../types';
import { getMockAuthenticatedUser, loginWithMockCredentials, logoutMockSession, registerWithMockData } from '../mocks/authMocks';

export class AuthService {
  /**
   * Realiza o cadastro de um novo usuário.
   */
  public static async register(data: RegisterRequestDTO): Promise<UserDTO> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.post<UserDTO, RegisterRequestDTO>('/auth/register', data);
    }

    return registerWithMockData(data);
  }

  /**
   * Realiza o login e obtém o token.
   */
  public static async login(credentials: LoginRequestDTO): Promise<LoginResponseDTO> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.post<LoginResponseDTO, LoginRequestDTO>('/auth/login', credentials);
    }

    return loginWithMockCredentials(credentials);
  }

  /**
   * Invalida o token atual no servidor (Blacklist via Redis).
   */
  public static async logout(): Promise<void> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.post<void, void>('/auth/logout');
    }

    return logoutMockSession();
  }

  /**
   * Obtém os dados do usuário logado (Perfil e Roles).
   */
  public static async getMe(): Promise<UserDTO> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.get<UserDTO>('/auth/users/me');
    }

    return getMockAuthenticatedUser();
  }
}