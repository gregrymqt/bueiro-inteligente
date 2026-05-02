import { apiClient } from '@/core/http/ApiClient';
import type { UserMessage } from '../types';

export class MessageService {
  private static readonly BASE_API = '/api/v1/messages';

  public static async getMessages(useMock: boolean = false): Promise<UserMessage[]> {
    if (!useMock) {
      return apiClient.get<UserMessage[]>(this.BASE_API);
    }
    // Mock simples caso a API não exista ainda
    return [
      {
        id: '1',
        name: 'João Silva',
        email: 'joao@example.com',
        message: 'Gostaria de saber mais sobre a solução.',
        created_at: new Date().toISOString(),
        is_read: false
      },
      {
        id: '2',
        name: 'Maria Santos',
        email: 'maria@example.com',
        message: 'Qual o valor de implantação em um condomínio?',
        created_at: new Date(Date.now() - 86400000).toISOString(),
        is_read: true
      }
    ];
  }

  public static async markAsRead(id: string, useMock: boolean = false): Promise<void> {
    if (!useMock) {
      return apiClient.patch<void>(`${this.BASE_API}/${id}/read`, {});
    }
  }

  public static async deleteMessage(id: string, useMock: boolean = false): Promise<void> {
    if (!useMock) {
      return apiClient.delete<void>(`${this.BASE_API}/${id}`);
    }
  }
}
