const DEFAULT_MOCK_DELAY_MS = 320;

const isTestMode = import.meta.env.MODE === 'test';

const wait = (durationMs: number): Promise<void> =>
  new Promise((resolve) => {
    globalThis.setTimeout(resolve, durationMs);
  });

export const withMockLatency = async <T>(factory: () => T | Promise<T>, delayMs: number = DEFAULT_MOCK_DELAY_MS): Promise<T> => {
  const effectiveDelay = isTestMode ? 0 : delayMs;

  if (effectiveDelay > 0) {
    await wait(effectiveDelay);
  }

  return await factory();
};