import { apiClient } from "@/core/http/ApiClient";
import { signalRClient } from "@/core/socket/SignalRClient";
import type { DrainStatus } from "../types";

export class MonitoringService {
  private static readonly BASE_API = '/api/v1/monitoring';
  
  public static async getInitialStatus(bueiroId: string): Promise<DrainStatus> {
    return apiClient.get<DrainStatus>(`${this.BASE_API}/${bueiroId}/status`);
  }

  // Avisa o servidor para começar a enviar dados DESTE bueiro
  public static async joinDrainGroup(bueiroId: string): Promise<void> {
    await signalRClient.invoke('JoinDrain', bueiroId);
  }

  // Avisa o servidor para parar de enviar dados DESTE bueiro
  public static async leaveDrainGroup(bueiroId: string): Promise<void> {
    await signalRClient.invoke('LeaveDrain', bueiroId);
  }

  public static subscribeToUpdates(onMessage: (payload: DrainStatus) => void): () => void {
    return signalRClient.subscribe<DrainStatus>('BUEIRO_STATUS_MUDOU', onMessage);
  }
}