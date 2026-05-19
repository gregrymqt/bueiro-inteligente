import { apiClient } from '@/core/http/ApiClient';
import type { CreatePixRequest, PixPaymentResponse, RetryPixRequest } from '../types/pix.type';


export class PixService {
    private static readonly BASE_API = '/api/v1/pix';

  /**
   * Envia a solicitação de criação de ordem Pix para o backend
   */
  public static async createOrder(request: CreatePixRequest): Promise<PixPaymentResponse> {
    const response = await apiClient.post<PixPaymentResponse>(
      `${this.BASE_API}/create-order`, 
      request
    );
    return response;
  }

  /**
   * Solicita a retentativa de processamento de um Pix
   */
  public static async retryOrder(request: RetryPixRequest): Promise<{ message: string }> {
    const response = await apiClient.put<{ message: string }>(
      `${this.BASE_API}/retry`, 
      request
    );
    return response;
  }
}