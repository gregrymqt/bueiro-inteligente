import { apiClient } from '@/core/http/ApiClient';
import { signalRClient } from '@/core/socket/SignalRClient';
import type { DrainStatus } from '../types';

export class MonitoringService {
  /**
   * Busca inicial (REST) - Padrão que você já usa
   */
  public static async getInitialStatus(bueiroId: string): Promise<DrainStatus> {
    return apiClient.get<DrainStatus>(`/monitoring/${bueiroId}/status`);
  }

  /**
   * Gerencia a inscrição no realtime via SignalR.
   * Mantém uma única conexão compartilhada entre os consumidores da feature.
   */
  public static subscribeToUpdates(onMessage: (payload: DrainStatus) => void): () => void {
    return signalRClient.subscribe<DrainStatus>('BUEIRO_STATUS_MUDOU', onMessage);
  }
}