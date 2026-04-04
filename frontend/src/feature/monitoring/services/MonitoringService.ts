import { apiClient } from '@/core/http/ApiClient';
import { TokenService } from '@/core/http/TokenService';
import type { DrainStatus, WSPayload } from '../types';

export class MonitoringService {
  private static socket: WebSocket | null = null;
  private static readonly WS_URL = import.meta.env.VITE_WS_URL || 'ws://localhost:8000/realtime/ws';

  /**
   * Busca inicial (REST) - Padrão que você já usa
   */
  public static async getInitialStatus(bueiroId: string): Promise<DrainStatus> {
    return apiClient.get<DrainStatus>(`/monitoring/${bueiroId}/status`);
  }

  /**
   * Gerencia a Inscrição no Real-time (WebSocket)
   * Centraliza a lógica de conexão para evitar múltiplos sockets abertos.
   */
  public static subscribeToUpdates(onMessage: (payload: WSPayload) => void): () => void {
    const tokenService = new TokenService();
    const token = tokenService.getToken();

    if (!this.socket || this.socket.readyState === WebSocket.CLOSED) {
      this.socket = new WebSocket(`${this.WS_URL}?token=${token}`);
      
      // Ping para manter a conexão ativa no Render
      this.socket.onopen = () => {
        const pingInterval = setInterval(() => {
          if (this.socket?.readyState === WebSocket.OPEN) {
            this.socket.send("ping");
          }
        }, 15000);

        this.socket?.addEventListener('close', () => clearInterval(pingInterval));
      };
    }

    const messageHandler = (event: MessageEvent) => {
      try {
        if (event.data === "pong") return; // Ignora o pong do servidor
        const payload: WSPayload = JSON.parse(event.data);
        onMessage(payload);
      } catch (err) {
        console.error("Erro no parse do WebSocket:", err);
      }
    };

    this.socket.addEventListener('message', messageHandler);

    // Retorna uma função de "unsubscribe" (limpeza)
    return () => {
      this.socket?.removeEventListener('message', messageHandler);
    };
  }
}