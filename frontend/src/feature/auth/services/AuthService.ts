import { apiClient } from '@/core/http/ApiClient';
import type { LoginRequestDTO, LoginResponseDTO, UserDTO, RegisterRequestDTO } from '../types';
import { getMockAuthenticatedUser, loginWithMockCredentials, logoutMockSession, registerWithMockData } from '../mocks/authMocks';
import { resolveBackendBaseUrl } from '@/core/http/environment';

export class AuthService {
  /**
   * Realiza o cadastro de um novo usuário.
   */
  public static async register(data: RegisterRequestDTO, useMock: boolean): Promise<UserDTO> {
    if (!useMock) {
      return apiClient.post<UserDTO, RegisterRequestDTO>(`${resolveBackendBaseUrl()}/auth/register`, data);
    }

    return registerWithMockData(data);
  }

  /**
   * Realiza o login e obtém o token.
   */
  public static async login(credentials: LoginRequestDTO, useMock: boolean): Promise<LoginResponseDTO> {
    if (!useMock) {
      return apiClient.post<LoginResponseDTO, LoginRequestDTO>(`${resolveBackendBaseUrl()}/auth/login`, credentials);
    }

    return loginWithMockCredentials(credentials);
  }

  /**
   * Invalida o token atual no servidor (Blacklist via Redis).
   */
  public static async logout(useMock: boolean): Promise<void> {
    if (!useMock) {
      return apiClient.post<void, void>(`${resolveBackendBaseUrl()}/auth/logout`);
    }

    return logoutMockSession();
  }

  /**
   * Obtém os dados do usuário logado (Perfil e Roles).
   */
  public static async getMe(useMock: boolean): Promise<UserDTO> {
    if (!useMock) {
      return apiClient.get<UserDTO>(`${resolveBackendBaseUrl()}/auth/users/me`);
    }

    return getMockAuthenticatedUser();
  }
}