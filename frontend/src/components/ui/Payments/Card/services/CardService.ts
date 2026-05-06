import { apiClient } from '@/core/http/ApiClient';
import type { 
  CreditCardRequest, 
  CreditCardResponse, 
  RetryCreditCardRequest 
} from '../types/card.types';

export const CardService = {
  /**
   * Envia o token e os dados do cartão para processamento no backend
   */
  async processPayment(request: CreditCardRequest): Promise<CreditCardResponse> {
    // Rota mapeada conforme CreditCardController.cs[cite: 24]
    return await apiClient.post<CreditCardResponse>('/credit-card/process', request);
  },

  /**
   * Solicita uma retentativa de pagamento (caso de cartão recusado ou erro)[cite: 24]
   */
  async retryPayment(request: RetryCreditCardRequest): Promise<{ message: string }> {
    return await apiClient.put<{ message: string }>('/credit-card/retry', request);
  }
};