import { apiClient } from '@/core/http/ApiClient';
import type { LoginRequestDTO, LoginResponseDTO, UserDTO, RegisterRequestDTO } from '../types';
import { getMockAuthenticatedUser, loginWithMockCredentials, logoutMockSession, registerWithMockData } from '../mocks/authMocks';

export class AuthService {
  private static readonly BASE_API = '/api/v1/auth';
  /**
   * Realiza o cadastro de um novo usuário.
   */
  public static async register(data: RegisterRequestDTO, useMock: boolean): Promise<UserDTO> {
    if (!useMock) {
      return apiClient.post<UserDTO, RegisterRequestDTO>(`${this.BASE_API}/register`, data);
    }

    return registerWithMockData(data);
  }

  /**
   * Realiza o login e obtém o token.
   */
  public static async login(credentials: LoginRequestDTO, useMock: boolean): Promise<LoginResponseDTO> {
    if (!useMock) {
      return apiClient.post<LoginResponseDTO, LoginRequestDTO>(`${this.BASE_API}/login`, credentials);
    }

    return loginWithMockCredentials(credentials);
  }

  /**
   * Invalida o token atual no servidor (Blacklist via Redis).
   */
  public static async logout(useMock: boolean): Promise<void> {
    if (!useMock) {
      return apiClient.post<void, void>(`${this.BASE_API}/logout`);
    }

    return logoutMockSession();
  }

  /**
   * Obtém os dados do usuário logado (Perfil e Roles).
   */
  public static async getMe(useMock: boolean): Promise<UserDTO> {
    if (!useMock) {
      return apiClient.get<UserDTO>(`${this.BASE_API}/users/me`);
    }

    return getMockAuthenticatedUser();
  }
}