import { apiClient } from '@/core/http/ApiClient';
import type { LoginRequestDTO, LoginResponseDTO } from '../types';

export class AuthService {
  /**
   * Envia as credenciais para a API e retorna o Token JWT.
   */
  public static async login(credentials: LoginRequestDTO): Promise<LoginResponseDTO> {
    // Usamos o nosso ApiClient genérico. 
    // Passamos o tipo de Resposta (LoginResponseDTO) e o tipo do Body (LoginRequestDTO)
    return apiClient.post<LoginResponseDTO, LoginRequestDTO>('/auth/login', credentials);
  }
}