import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const alertMocks = vi.hoisted(() => ({
  error: vi.fn(),
  warning: vi.fn(),
  success: vi.fn(),
  confirm: vi.fn(),
}));

vi.mock('../../alert/AlertService', () => ({
  AlertService: {
    error: alertMocks.error,
    warning: alertMocks.warning,
    success: alertMocks.success,
    confirm: alertMocks.confirm,
  },
}));

const signalRMocks = vi.hoisted(() => {
  const state: {
    oncloseHandler: ((error?: unknown) => void) | null;
    connection: {
      state: string;
      start: ReturnType<typeof vi.fn>;
      on: ReturnType<typeof vi.fn>;
      off: ReturnType<typeof vi.fn>;
      onclose: ReturnType<typeof vi.fn>;
    };
    builder: {
      withUrl: ReturnType<typeof vi.fn>;
      withAutomaticReconnect: ReturnType<typeof vi.fn>;
      configureLogging: ReturnType<typeof vi.fn>;
      build: ReturnType<typeof vi.fn>;
    };
  } = {
    oncloseHandler: null,
    connection: {
      state: 'Disconnected',
      start: vi.fn(),
      on: vi.fn(),
      off: vi.fn(),
      onclose: vi.fn((handler: (error?: unknown) => void) => {
        state.oncloseHandler = handler;
      }),
    },
    builder: {
      withUrl: vi.fn().mockReturnThis(),
      withAutomaticReconnect: vi.fn().mockReturnThis(),
      configureLogging: vi.fn().mockReturnThis(),
      build: vi.fn(() => state.connection),
    },
  };

  return state;
});

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: class HubConnectionBuilderMock {
    public withUrl(...args: unknown[]) {
      signalRMocks.builder.withUrl(...args);
      return this;
    }

    public withAutomaticReconnect(...args: unknown[]) {
      signalRMocks.builder.withAutomaticReconnect(...args);
      return this;
    }

    public configureLogging(...args: unknown[]) {
      signalRMocks.builder.configureLogging(...args);
      return this;
    }

    public build(...args: unknown[]) {
      signalRMocks.builder.build(...args);
      return signalRMocks.connection;
    }
  },
  HubConnectionState: {
    Connected: 'Connected',
    Connecting: 'Connecting',
    Disconnected: 'Disconnected',
    Reconnecting: 'Reconnecting',
  },
  HttpTransportType: {
    LongPolling: 'LongPolling',
  },
  LogLevel: {
    Information: 'Information',
  },
}));

describe('SignalRClient', () => {
  beforeEach(() => {
    vi.resetModules();
    vi.clearAllMocks();
    signalRMocks.oncloseHandler = null;
    signalRMocks.connection.state = 'Disconnected';
    signalRMocks.connection.start.mockResolvedValue(undefined);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('resolve a URL local quando o backend aponta para localhost', async () => {
    const module = await import('../SignalRClient');

    expect(
      module.resolveSignalRHubUrl({
        VITE_BACKEND_URL: 'http://localhost:8080',
      }),
    ).toBe('http://localhost:8080/realtime/ws');

    expect(
      module.resolveSignalRHubUrl({
        VITE_BACKEND_LOCAL: 'TRUE',
      }),
    ).toBe('http://localhost:8000/realtime/ws');

    expect(
      module.resolveSignalRHubUrl({
        VITE_BACKEND_URL: 'https://example.com',
        VITE_WS_URL: 'https://example.com/realtime/ws',
      }),
    ).toBe('https://example.com/realtime/ws');
  });

  it('deriva o websocket a partir de uma URL de tunel', async () => {
    const module = await import('../SignalRClient');

    expect(
      module.resolveSignalRHubUrl({
        VITE_BACKEND_URL: 'https://abc.ngrok-free.app/',
        VITE_WS_URL: 'https://another.example.com/realtime/ws',
      }),
    ).toBe('https://abc.ngrok-free.app/realtime/ws');
  });

  it('cria uma única conexão compartilhada e inicia apenas uma vez', async () => {
    const module = await import('../SignalRClient');
    const firstHandler = vi.fn();
    const secondHandler = vi.fn();

    const firstUnsubscribe = module.signalRClient.subscribe('BUEIRO_STATUS_MUDOU', firstHandler);
    const secondUnsubscribe = module.signalRClient.subscribe('BUEIRO_STATUS_MUDOU', secondHandler);

    expect(signalRMocks.builder.withUrl).toHaveBeenCalledTimes(1);
    expect(signalRMocks.builder.withUrl).toHaveBeenCalledWith(
      'http://localhost:8000/realtime/ws',
      expect.objectContaining({
        headers: expect.objectContaining({
          'X-App-Id': expect.any(String),
        }),
        transport: 'LongPolling',
      }),
    );
    expect(signalRMocks.builder.withAutomaticReconnect).toHaveBeenCalledTimes(1);
    expect(signalRMocks.builder.configureLogging).toHaveBeenCalledTimes(1);
    expect(signalRMocks.builder.build).toHaveBeenCalledTimes(1);
    expect(signalRMocks.connection.start).toHaveBeenCalledTimes(1);
    expect(signalRMocks.connection.on).toHaveBeenCalledTimes(2);

    firstUnsubscribe();
    secondUnsubscribe();

    expect(signalRMocks.connection.off).toHaveBeenCalledTimes(2);
  });

  it('mostra erro quando a conexão encerra com falha', async () => {
    const module = await import('../SignalRClient');

    module.signalRClient.subscribe('BUEIRO_STATUS_MUDOU', vi.fn());
    signalRMocks.oncloseHandler?.(new Error('connection lost'));

    expect(alertMocks.error).toHaveBeenCalledWith(
      'Erro de Conexão',
      'Falha ao conectar no realtime do bueiro.',
    );
  });
});