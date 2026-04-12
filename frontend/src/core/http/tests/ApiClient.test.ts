import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { AlertService } from '../../alert/AlertService';
import { ApiClient } from '../ApiClient';
import { RATE_LIMIT_THROTTLED_EVENT, type IRateLimitService } from '../RateLimitService';
import type { ITokenService } from '../TokenService';

describe('ApiClient throttling', () => {
  const fetchMock = vi.fn();
  const tokenServiceMock: ITokenService = {
    getToken: vi.fn(() => null),
    saveToken: vi.fn(),
    removeToken: vi.fn(),
    isAuthenticated: vi.fn(() => false),
  };

  let rateLimitServiceMock: IRateLimitService;

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock);
    fetchMock.mockReset();
    rateLimitServiceMock = {
      isEnabled: true,
      checkLimit: vi.fn(() => false),
    };
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it('interrompe a chamada antes do fetch e dispara o aviso', async () => {
    const warningSpy = vi.spyOn(AlertService, 'warning').mockImplementation(() => undefined);
    const throttledEventHandler = vi.fn();
    window.addEventListener(RATE_LIMIT_THROTTLED_EVENT, throttledEventHandler as EventListener);

    const apiClient = new ApiClient('https://api.example.com', tokenServiceMock, rateLimitServiceMock);

    await expect(apiClient.get('/monitoring')).rejects.toThrow(RATE_LIMIT_THROTTLED_EVENT);

    expect(fetchMock).not.toHaveBeenCalled();
    expect(warningSpy).toHaveBeenCalledWith(
      'Muitas requisições',
      'Por favor, aguarde 1 minuto para não sobrecarregar o servidor.',
    );
    expect(throttledEventHandler).toHaveBeenCalledTimes(1);

    window.removeEventListener(RATE_LIMIT_THROTTLED_EVENT, throttledEventHandler as EventListener);
  });
});