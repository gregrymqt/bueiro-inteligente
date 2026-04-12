import { apiClient } from '@/core/http/ApiClient';
import { AlertService } from '@/core/alert/AlertService';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import type { DrainStatus } from '../types';

export class MonitoringService {
  private static connection: HubConnection | null = null;
  private static startPromise: Promise<void> | null = null;
  private static readonly WS_URL = `${import.meta.env.VITE_BACKEND_URL || 'http://localhost:8000'}/realtime/ws`;

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
    const connection = this.getConnection();
    const handler = (payload: DrainStatus) => {
      onMessage(payload);
    };

    connection.on('BUEIRO_STATUS_MUDOU', handler);
    this.ensureStarted(connection);

    return () => {
      connection.off('BUEIRO_STATUS_MUDOU', handler);
    };
  }

  private static getConnection(): HubConnection {
    if (this.connection) {
      return this.connection;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(this.WS_URL)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.onclose((error) => {
      if (error) {
        console.error('Conexão SignalR encerrada com erro:', error);
      }
    });

    return this.connection;
  }

  private static ensureStarted(connection: HubConnection): void {
    if (connection.state !== HubConnectionState.Disconnected || this.startPromise) {
      return;
    }

    this.startPromise = connection
      .start()
      .catch((error) => {
        console.error('Falha ao iniciar a conexão SignalR:', error);
        AlertService.error('Erro de Conexão', 'Falha ao conectar no realtime do bueiro.');
      })
      .finally(() => {
        this.startPromise = null;
      });
  }
}