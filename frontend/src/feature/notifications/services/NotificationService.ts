import { apiClient } from "@/core/http/ApiClient";

export class NotificationService {
  private static readonly BASE_API = "/api/v1/notifications";

  // Busca o resumo (lista + contador de não lidas) definido no Backend
  public static async getUnreadCount(): Promise<number> {
    // No backend, o NotificationSummaryDTO já retorna esse valor
    const response = await apiClient.get<{ unreadCount: number }>(this.BASE_API);
    return response.unreadCount;
  }
}