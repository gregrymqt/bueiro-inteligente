import { AlertService } from '../alert/AlertService';
import { resolveBackendBaseUrl } from './environment';
import {
  RATE_LIMIT_THROTTLED_EVENT,
  rateLimitService as defaultRateLimitService,
  type IRateLimitService,
} from './RateLimitService';
import { type ITokenService, tokenService } from './TokenService';

// 1. O Contrato do nosso Wrapper
export interface IApiClient {
  get<TResponse>(url: string): Promise<TResponse>;
  post<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse>;
  put<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse>;
  patch<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse>;
  delete<TResponse>(url: string): Promise<TResponse>;
}

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

// 2. A Implementação Concreta
export class ApiClient implements IApiClient {
  private readonly baseUrl: string;
  private readonly tokenService: ITokenService;
  private readonly rateLimitService: IRateLimitService;

  constructor(
    baseUrl: string,
    tokenService: ITokenService,
    rateLimitService: IRateLimitService = defaultRateLimitService,
  ) {
    this.baseUrl = baseUrl;
    this.tokenService = tokenService;
    this.rateLimitService = rateLimitService;
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

  private rejectRateLimitedRequest(): never {
    AlertService.warning(
      'Muitas requisições',
      'Por favor, aguarde 1 minuto para não sobrecarregar o servidor.',
    );
    window.dispatchEvent(new Event(RATE_LIMIT_THROTTLED_EVENT));
    throw new Error(RATE_LIMIT_THROTTLED_EVENT);
  }

  private enforceRateLimit(): void {
    if (!this.rateLimitService.checkLimit()) {
      this.rejectRateLimitedRequest();
    }
  }

  // Método privado para tratar as respostas e disparar erros centralizados
  private async handleResponse<TResponse>(response: Response): Promise<TResponse> {
    if (!response.ok) {
      // Se o token for inválido/expirado, o backend retornará 401
      if (response.status === 401) {
        this.tokenService.removeToken();
        // Disparar evento para forçar o usuário para a tela de Login
        window.dispatchEvent(new Event('auth:unauthorized'));
      }

      if (response.status === 429) {
        this.rejectRateLimitedRequest();
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

  private async executeRequest<TResponse, TBody = unknown>(
    method: HttpMethod,
    url: string,
    body?: TBody,
  ): Promise<TResponse> {
    this.enforceRateLimit();

    const requestInit: RequestInit = {
      method,
      headers: this.getHeaders(),
    };

    if (body !== undefined) {
      requestInit.body = JSON.stringify(body);
    }

    const response = await fetch(`${this.baseUrl}${url}`, requestInit);
    return this.handleResponse<TResponse>(response);
  }

  // ==========================================
  // MÉTODOS GENÉRICOS (CRUD REST)
  // ==========================================

  public async get<TResponse>(url: string): Promise<TResponse> {
    return this.executeRequest<TResponse>('GET', url);
  }

  public async post<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse> {
    return this.executeRequest<TResponse, TBody>('POST', url, body);
  }

  public async put<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse> {
    return this.executeRequest<TResponse, TBody>('PUT', url, body);
  }

  public async patch<TResponse, TBody = unknown>(url: string, body?: TBody): Promise<TResponse> {
    return this.executeRequest<TResponse, TBody>('PATCH', url, body);
  }

  public async delete<TResponse>(url: string): Promise<TResponse> {
    return this.executeRequest<TResponse>('DELETE', url);
  }
}

// Exportamos a instância já configurada com a URL base do seu ambiente
const API_BASE_URL = resolveBackendBaseUrl();
export const apiClient = new ApiClient(API_BASE_URL, tokenService, defaultRateLimitService);
