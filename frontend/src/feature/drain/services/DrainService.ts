import { apiClient } from '@/core/http/ApiClient';
import type { Drain, DrainCreatePayload, DrainUpdatePayload } from '../types';
import { createMockDrain, deleteMockDrain, getMockDrainById, getMockDrains, updateMockDrain } from '../mocks/drainMocks';

export class DrainService {
  public static async getDrains(useMock: boolean): Promise<Drain[]> {
    if (!useMock) {
      return apiClient.get<Drain[]>('/drains');
    }

    return getMockDrains();
  }

  public static async getDrainById(id: string, useMock: boolean): Promise<Drain> {
    if (!useMock) {
      return apiClient.get<Drain>(`/drains/${id}`);
    }

    return getMockDrainById(id);
  }

  public static async createDrain(data: DrainCreatePayload, useMock: boolean): Promise<Drain> {
    if (!useMock) {
      return apiClient.post<Drain, DrainCreatePayload>('/drains', data);
    }

    return createMockDrain(data);
  }

  public static async updateDrain(id: string, data: DrainUpdatePayload, useMock: boolean): Promise<Drain> {
    if (!useMock) {
      return apiClient.put<Drain, DrainUpdatePayload>(`/drains/${id}`, data);
    }

    return updateMockDrain(id, data);
  }

  public static async deleteDrain(id: string, useMock: boolean): Promise<void> {
    if (!useMock) {
      return apiClient.delete<void>(`/drains/${id}`);
    }

    return deleteMockDrain(id);
  }
}