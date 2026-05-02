import { apiClient } from '@/core/http/ApiClient';
import type { ContactMessage } from '../types';

export class MessageService {
  static async getMessages(useMock: boolean = false): Promise<ContactMessage[]> {
    if (useMock) {
      return new Promise((resolve) => {
        setTimeout(() => {
          resolve([
            {
              id: '1',
              name: 'João Silva',
              email: 'joao@example.com',
              subject: 'Dúvida sobre o plano Enterprise',
              message: 'Gostaria de saber mais sobre a integração via API.',
              createdAt: new Date().toISOString(),
              isRead: false,
            },
            {
              id: '2',
              name: 'Prefeitura M. SP',
              email: 'contato@prefeitura.sp.gov.br',
              subject: 'Parceria',
              message: 'Temos interesse em um projeto piloto na Zona Sul.',
              createdAt: new Date(Date.now() - 86400000).toISOString(),
              isRead: true,
            }
          ]);
        }, 500);
      });
    }

    return apiClient.get<ContactMessage[]>('/messages');
  }

  static async markAsRead(id: string, useMock: boolean = false): Promise<void> {
    if (useMock) {
      return new Promise((resolve) => setTimeout(resolve, 300));
    }
    await apiClient.put(`/messages/${id}/read`, {});
  }

  static async deleteMessage(id: string, useMock: boolean = false): Promise<void> {
    if (useMock) {
      return new Promise((resolve) => setTimeout(resolve, 300));
    }
    await apiClient.delete(`/messages/${id}`);
  }
}
