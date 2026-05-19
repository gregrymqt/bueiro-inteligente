import { apiClient } from '@/core/http/ApiClient';
import type {
  CreditCardRequest,
  CreditCardResponse,
} from '../types/card.types';

export class CardService {
  private static readonly BASE_API = '/api/v1/creditcard';
  /**
   * Envia o token e os dados do cartão para processamento no backend
   */
  public static async processPayment(request: CreditCardRequest): Promise<CreditCardResponse> {
    // Rota mapeada conforme CreditCardController.cs[cite: 24]
    return await apiClient.post<CreditCardResponse>(`${this.BASE_API}/process`, request);
  }

  /**
   * Solicita uma retentativa de pagamento (caso de cartão recusado ou erro)[cite: 24]
   */
  public static async retryPayment(request: CreditCardRequest): Promise<{ message: string }> {
    return await apiClient.put<{ message: string }>(`${this.BASE_API}/retry`, request);
  }
};