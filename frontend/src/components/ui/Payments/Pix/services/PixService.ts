import { apiClient } from '@/core/http/ApiClient';
import type { CreatePixRequest, PixPaymentResponse, RetryPixRequest } from '../types/pix.type';


export const PixService = {
  /**
   * Envia a solicitação de criação de ordem Pix para o backend
   */
  async createOrder(request: CreatePixRequest): Promise<PixPaymentResponse> {
    const response = await apiClient.post<PixPaymentResponse>(
      '/pix/create-order', 
      request
    );
    return response;
  },

  /**
   * Solicita a retentativa de processamento de um Pix
   */
  async retryOrder(request: RetryPixRequest): Promise<{ message: string }> {
    const response = await apiClient.put<{ message: string }>(
      '/pix/retry', 
      request
    );
    return response;
  }
};