import { apiClient } from '@/core/http/ApiClient';
import type { DrainStatusDTO } from '../types';

export class MonitoringService {
  /**
   * Busca o status em tempo real do bueiro no Redis via FastAPI.
   */
  public static async getDrainStatus(bueiroId: string): Promise<DrainStatusDTO> {
    // A rota exata do endpoint. Se amanhã a API mudar para /v2/monitoring, 
    // é só alterar aqui e o resto do sistema continua intacto.
    return apiClient.get<DrainStatusDTO>(`/monitoring/${bueiroId}/status`);
  }
}