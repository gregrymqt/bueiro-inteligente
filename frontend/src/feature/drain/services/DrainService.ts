import { apiClient } from '@/core/http/ApiClient';
import type { Drain, DrainCreatePayload, DrainUpdatePayload } from '../types';
import { createMockDrain, deleteMockDrain, getMockDrainById, getMockDrains, updateMockDrain } from '../mocks/drainMocks';

export class DrainService {
    private static readonly BASE_API = '/api/v1/drains';

  public static async getDrains(useMock: boolean): Promise<Drain[]> {
    if (!useMock) {
      return apiClient.get<Drain[]>(this.BASE_API);
    }

    return getMockDrains();
  }

  public static async getDrainById(id: string, useMock: boolean): Promise<Drain> {
    if (!useMock) {
      return apiClient.get<Drain>(`${this.BASE_API}/${id}`);
    }

    return getMockDrainById(id);
  }

  public static async createDrain(data: DrainCreatePayload, useMock: boolean): Promise<Drain> {
    if (!useMock) {
      return apiClient.post<Drain, DrainCreatePayload>(this.BASE_API, data);
    }

    return createMockDrain(data);
  }

  public static async updateDrain(id: string, data: DrainUpdatePayload, useMock: boolean): Promise<Drain> {
    if (!useMock) {
      return apiClient.put<Drain, DrainUpdatePayload>(`${this.BASE_API}/${id}`, data);
    }

    return updateMockDrain(id, data);
  }

  public static async deleteDrain(id: string, useMock: boolean): Promise<void> {
    if (!useMock) {
      return apiClient.delete<void>(`${this.BASE_API}/${id}`);
    }

    return deleteMockDrain(id);
  }
}