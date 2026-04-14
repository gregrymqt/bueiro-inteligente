import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { AlertService } from '../alert/AlertService';
import { resolveBackendBaseUrl, resolveUrlMode } from '../http/environment';

export type SignalRHubEnvironment = Pick<
  ImportMetaEnv,
  'VITE_BACKEND_LOCAL' | 'VITE_BACKEND_URL' | 'VITE_WS_LOCAL' | 'VITE_WS_URL'
>;

export type SignalRMessageHandler<TPayload> = (payload: TPayload) => void;

export const SIGNALR_LOCAL_URL = 'http://localhost:8000/realtime/ws';
export const SIGNALR_CONNECTION_ERROR_TITLE = 'Erro de Conexão';
export const SIGNALR_CONNECTION_ERROR_TEXT = 'Falha ao conectar no realtime do bueiro.';

export function resolveSignalRHubUrl(
  env: SignalRHubEnvironment = import.meta.env,
): string {
  const backendUrl = resolveBackendBaseUrl(env);
  const backendMode = resolveUrlMode(backendUrl);
  const wsUrl = normalizeUrl(env.VITE_WS_URL);

  if (backendMode === 'local' || backendMode === 'tunnel') {
    return appendRealtimePath(backendUrl);
  }

  if (isTruthyFlag(env.VITE_WS_LOCAL) || isTruthyFlag(env.VITE_BACKEND_LOCAL)) {
    return SIGNALR_LOCAL_URL;
  }

  if (wsUrl) {
    return wsUrl;
  }

  return appendRealtimePath(backendUrl);
}

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
      .withUrl(resolveSignalRHubUrl())
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

function isTruthyFlag(value?: string): boolean {
  return value?.trim().toUpperCase() === 'TRUE';
}

function normalizeUrl(value?: string): string | null {
  const normalized = value?.trim().replace(/\/+$/, '');

  return normalized ? normalized : null;
}

function appendRealtimePath(baseUrl: string): string {
  const normalizedBase = baseUrl.replace(/\/+$/, '');

  if (normalizedBase.endsWith('/realtime/ws')) {
    return normalizedBase;
  }

  return `${normalizedBase}/realtime/ws`;
}

export const signalRClient = SignalRClient.getInstance();