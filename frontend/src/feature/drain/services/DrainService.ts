import { apiClient } from '@/core/http/ApiClient';
import { isMockDataSourceEnabled } from '@/core/http/environment';
import type { Drain, DrainCreatePayload, DrainUpdatePayload } from '../types';
import { createMockDrain, deleteMockDrain, getMockDrainById, getMockDrains, updateMockDrain } from '../mocks/drainMocks';

export class DrainService {
  public static async getDrains(): Promise<Drain[]> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.get<Drain[]>('/drains');
    }

    return getMockDrains();
  }

  public static async getDrainById(id: string): Promise<Drain> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.get<Drain>(`/drains/${id}`);
    }

    return getMockDrainById(id);
  }

  public static async createDrain(data: DrainCreatePayload): Promise<Drain> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.post<Drain, DrainCreatePayload>('/drains', data);
    }

    return createMockDrain(data);
  }

  public static async updateDrain(id: string, data: DrainUpdatePayload): Promise<Drain> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.put<Drain, DrainUpdatePayload>(`/drains/${id}`, data);
    }

    return updateMockDrain(id, data);
  }

  public static async deleteDrain(id: string): Promise<void> {
    if (!isMockDataSourceEnabled()) {
      return apiClient.delete<void>(`/drains/${id}`);
    }

    return deleteMockDrain(id);
  }
}