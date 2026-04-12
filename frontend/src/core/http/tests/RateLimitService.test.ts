import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { RateLimitService } from '../RateLimitService';

describe('RateLimitService', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(0);
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it('permite todas as requisições quando desabilitado', () => {
    const service = new RateLimitService(false);

    for (let index = 0; index < 20; index += 1) {
      expect(service.checkLimit()).toBe(true);
    }
  });

  it('respeita a janela deslizante de 10 segundos', () => {
    const service = new RateLimitService(true);

    for (let index = 0; index < 10; index += 1) {
      expect(service.checkLimit()).toBe(true);
    }

    vi.advanceTimersByTime(10_001);

    expect(service.checkLimit()).toBe(true);
  });

  it('ativa lockout ao exceder o limite e libera após 1 minuto', () => {
    const service = new RateLimitService(true);

    for (let index = 0; index < 10; index += 1) {
      expect(service.checkLimit()).toBe(true);
    }

    expect(service.checkLimit()).toBe(false);

    vi.advanceTimersByTime(60_000);

    expect(service.checkLimit()).toBe(true);
  });
});