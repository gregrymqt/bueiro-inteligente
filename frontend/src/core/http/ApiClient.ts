import { type ITokenService, tokenService } from './TokenService';

// 1. O Contrato do nosso Wrapper
export interface IApiClient {
  get<TResponse>(url: string): Promise<TResponse>;
  post<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse>;
  put<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse>;
  patch<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse>;
  delete<TResponse>(url: string): Promise<TResponse>;
}

// 2. A Implementação Concreta
export class ApiClient implements IApiClient {
  private baseUrl: string;
  private tokenService: ITokenService;

  constructor(baseUrl: string, tokenService: ITokenService) {
    this.baseUrl = baseUrl;
    this.tokenService = tokenService;
  }

  // Método privado para montar os cabeçalhos padrão e injetar o JWT
  private getHeaders(): HeadersInit {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };

    const token = this.tokenService.getToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    return headers;
  }

  // Método privado para tratar as respostas e disparar erros centralizados
  private async handleResponse<TResponse>(response: Response): Promise<TResponse> {
    if (!response.ok) {
      // Se o token for inválido/expirado, o FastAPI retornará 401
      if (response.status === 401) {
        this.tokenService.removeToken();
        // Disparar evento para forçar o usuário para a tela de Login
        window.dispatchEvent(new Event('auth:unauthorized')); 
      }
      
      const errorData = await response.json().catch(() => null);
      throw new Error(errorData?.detail || `Erro HTTP: ${response.status}`);
    }

    // Se a resposta for 204 (No Content), retornamos vazio sem tentar fazer o parse
    if (response.status === 204) {
      return {} as TResponse;
    }

    return await response.json() as TResponse;
  }

  // ==========================================
  // MÉTODOS GENÉRICOS (CRUD REST)
  // ==========================================

  public async get<TResponse>(url: string): Promise<TResponse> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: 'GET',
      headers: this.getHeaders(),
    });
    return this.handleResponse<TResponse>(response);
  }

  public async post<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(body),
    });
    return this.handleResponse<TResponse>(response);
  }

  public async put<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: 'PUT',
      headers: this.getHeaders(),
      body: JSON.stringify(body),
    });
    return this.handleResponse<TResponse>(response);
  }

  public async patch<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: 'PATCH',
      headers: this.getHeaders(),
      body: body ? JSON.stringify(body) : undefined,
    });
    return this.handleResponse<TResponse>(response);
  }

  public async delete<TResponse>(url: string): Promise<TResponse> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: 'DELETE',
      headers: this.getHeaders(),
    });
    return this.handleResponse<TResponse>(response);
  }
}

// Exportamos a instância já configurada com a URL base do seu ambiente
const API_BASE_URL = 'http://localhost:8000';
export const apiClient = new ApiClient(API_BASE_URL, tokenService);
