import { apiClient } from '@/core/http/ApiClient';
import type { CreatePreferenceRequest, PreferenceResponse } from '../types/preference.types';

export const PreferenceService = {
  /**
   * Solicita a criação de uma Preference (Checkout Pro / Wallet) no backend
   */
  async createPreference(request: CreatePreferenceRequest): Promise<PreferenceResponse> {
    return await apiClient.post<PreferenceResponse>('/preference/create', request);
  }
};