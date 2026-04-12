export const RATE_LIMIT_THROTTLED_EVENT = 'api:throttled';

export interface IRateLimitService {
  readonly isEnabled: boolean;
  checkLimit(): boolean;
}

export class RateLimitService implements IRateLimitService {
  private static readonly MAX_REQUESTS = 10;
  private static readonly WINDOW_MS = 10_000;
  private static readonly LOCKOUT_MS = 60_000;

  public readonly isEnabled: boolean;

  private requestTimestamps: number[] = [];
  private lockoutUntil = 0;
  private lockoutTimeoutId: ReturnType<typeof setTimeout> | null = null;

  constructor(isEnabled: boolean = RateLimitService.readIsEnabled()) {
    this.isEnabled = isEnabled;
  }

  public checkLimit(): boolean {
    if (!this.isEnabled) {
      return true;
    }

    const now = Date.now();

    if (this.lockoutUntil !== 0 && now >= this.lockoutUntil) {
      this.resetLockoutState();
    }

    if (this.lockoutUntil > now) {
      return false;
    }

    this.requestTimestamps = this.requestTimestamps.filter(
      (timestamp) => now - timestamp < RateLimitService.WINDOW_MS,
    );

    if (this.requestTimestamps.length < RateLimitService.MAX_REQUESTS) {
      this.requestTimestamps.push(now);
      return true;
    }

    this.startLockout(now);
    return false;
  }

  private startLockout(now: number): void {
    this.lockoutUntil = now + RateLimitService.LOCKOUT_MS;
    this.requestTimestamps = [];

    if (this.lockoutTimeoutId !== null) {
      clearTimeout(this.lockoutTimeoutId);
    }

    this.lockoutTimeoutId = setTimeout(() => {
      this.resetLockoutState();
    }, RateLimitService.LOCKOUT_MS);
  }

  private resetLockoutState(): void {
    this.lockoutUntil = 0;
    this.requestTimestamps = [];

    if (this.lockoutTimeoutId !== null) {
      clearTimeout(this.lockoutTimeoutId);
      this.lockoutTimeoutId = null;
    }
  }

  private static readIsEnabled(): boolean {
    return RateLimitService.parseBoolean(import.meta.env.VITE_ENABLE_RATE_LIMIT);
  }

  private static parseBoolean(value?: string): boolean {
    if (!value) {
      return false;
    }

    const normalizedValue = value.trim().toLowerCase();
    return ['true', '1', 'yes', 'on'].includes(normalizedValue);
  }
}

export const rateLimitService = new RateLimitService();