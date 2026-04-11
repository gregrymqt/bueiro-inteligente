import { apiClient } from '@/core/http/ApiClient';
import type { Drain, DrainCreatePayload, DrainUpdatePayload } from '../types';

export class DrainService {
  private static readonly basePath = '/admin/drains';

  public static async getDrains(): Promise<Drain[]> {
    return apiClient.get<Drain[]>(this.basePath);
  }

  public static async getDrainById(id: string): Promise<Drain> {
    return apiClient.get<Drain>(`${this.basePath}/${id}`);
  }

  public static async createDrain(data: DrainCreatePayload): Promise<Drain> {
    return apiClient.post<Drain, DrainCreatePayload>(this.basePath, data);
  }

  public static async updateDrain(id: string, data: DrainUpdatePayload): Promise<Drain> {
    return apiClient.patch<Drain, DrainUpdatePayload>(`${this.basePath}/${id}`, data);
  }

  public static async deleteDrain(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.basePath}/${id}`);
  }
}