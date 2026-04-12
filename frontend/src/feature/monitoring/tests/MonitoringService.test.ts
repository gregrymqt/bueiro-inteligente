import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('@/core/http/ApiClient', () => ({
  apiClient: {
    get: vi.fn(),
  },
}));

const monitoringMocks = vi.hoisted(() => ({
  signalRSubscribeMock: vi.fn(),
}));

vi.mock('@/core/socket/SignalRClient', () => ({
  signalRClient: {
    subscribe: monitoringMocks.signalRSubscribeMock,
  },
}));

describe('MonitoringService', () => {
  beforeEach(() => {
    vi.resetModules();
    vi.clearAllMocks();
    monitoringMocks.signalRSubscribeMock.mockReturnValue(vi.fn());
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('registra o handler de realtime no cliente global', async () => {
    const { MonitoringService } = await import('../services/MonitoringService');
    const onMessage = vi.fn();

    const unsubscribe = MonitoringService.subscribeToUpdates(onMessage);

    expect(monitoringMocks.signalRSubscribeMock).toHaveBeenCalledWith(
      'BUEIRO_STATUS_MUDOU',
      onMessage,
    );
    expect(unsubscribe).toBeTypeOf('function');
  });
});