import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  HttpTransportType,
  LogLevel,
} from '@microsoft/signalr';
import { AlertService } from '../alert/AlertService';
import { resolveAppId, resolveSignalRHubUrl } from '../http/environment';

export type SignalRMessageHandler<TPayload> = (payload: TPayload) => void;
export const SIGNALR_CONNECTION_ERROR_TITLE = 'Erro de Conexão';
export const SIGNALR_CONNECTION_ERROR_TEXT = 'Falha ao conectar no realtime do bueiro.';

export class SignalRClient {
  private static instance: SignalRClient | null = null;

  private connection: HubConnection | null = null;
  private startPromise: Promise<void> | null = null;

  private constructor() {}

  public static getInstance(): SignalRClient {
    if (SignalRClient.instance === null) {
      SignalRClient.instance = new SignalRClient();
    }

    return SignalRClient.instance;
  }

  public subscribe<TPayload>(eventName: string, handler: SignalRMessageHandler<TPayload>): () => void {
    const connection = this.getConnection();
    connection.on(eventName, handler);
    void this.ensureStarted();

    return () => {
      connection.off(eventName, handler);
    };
  }

  private getConnection(): HubConnection {
    if (this.connection !== null) {
      return this.connection;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(resolveSignalRHubUrl(), {
        headers: {
          'X-App-Id': resolveAppId(),
        },
        transport: HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.onclose((error: unknown) => {
      if (error) {
        this.handleConnectionError(error);
      }
    });

    return this.connection;
  }

  private ensureStarted(): Promise<void> {
    const connection = this.getConnection();

    if (this.startPromise !== null || connection.state !== HubConnectionState.Disconnected) {
      return this.startPromise ?? Promise.resolve();
    }

    this.startPromise = connection
      .start()
      .catch((error: unknown) => {
        this.handleConnectionError(error);
      })
      .finally(() => {
        this.startPromise = null;
      });

    return this.startPromise ?? Promise.resolve();
  }

  private handleConnectionError(error: unknown): void {
    console.error('SignalR connection error:', error);
    AlertService.error(SIGNALR_CONNECTION_ERROR_TITLE, SIGNALR_CONNECTION_ERROR_TEXT);
  }
}

export const signalRClient = SignalRClient.getInstance();