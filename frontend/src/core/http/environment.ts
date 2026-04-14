export type BackendEnvironment = Pick<
  ImportMetaEnv,
  'VITE_BACKEND_URL' | 'VITE_BACKEND_LOCAL' | 'VITE_DATA_SOURCE'
>;

export const LOCAL_BACKEND_URL = 'http://localhost:8000';

export type UrlMode = 'local' | 'tunnel' | 'remote';
export type AppDataSourceMode = 'mock' | 'backend';

function isTruthyFlag(value?: string): boolean {
  return value?.trim().toUpperCase() === 'TRUE';
}

function normalizeDataSourceMode(value?: string): AppDataSourceMode {
  const normalizedValue = value?.trim().toLowerCase();

  if (normalizedValue === 'backend' || normalizedValue === 'real' || normalizedValue === 'api') {
    return 'backend';
  }

  return 'mock';
}

function normalizeUrl(value?: string): string | null {
  const normalized = value?.trim().replace(/\/+$/, '');

  return normalized ? normalized : null;
}

function getHostname(urlValue: string): string | null {
  try {
    return new URL(urlValue).hostname.toLowerCase();
  } catch {
    return null;
  }
}

function isLocalhostHost(hostname: string): boolean {
  return (
    hostname === 'localhost'
    || hostname === '127.0.0.1'
    || hostname === '::1'
    || hostname.endsWith('.localhost')
  );
}

function isTunnelHost(hostname: string): boolean {
  return (
    hostname.includes('ngrok')
    || hostname.includes('grok')
    || hostname.includes('localtunnel')
    || hostname.includes('serveo')
    || hostname.includes('pagekite')
    || hostname.includes('tunnel')
  );
}

export function resolveUrlMode(urlValue?: string): UrlMode {
  const normalizedUrl = normalizeUrl(urlValue);

  if (!normalizedUrl) {
    return 'remote';
  }

  const hostname = getHostname(normalizedUrl);

  if (!hostname) {
    return 'remote';
  }

  if (isLocalhostHost(hostname)) {
    return 'local';
  }

  if (isTunnelHost(hostname)) {
    return 'tunnel';
  }

  return 'remote';
}

export function resolveBackendBaseUrl(
  env: BackendEnvironment = import.meta.env,
): string {
  const backendUrl = normalizeUrl(env.VITE_BACKEND_URL);

  if (!backendUrl) {
    if (isTruthyFlag(env.VITE_BACKEND_LOCAL)) {
      return LOCAL_BACKEND_URL;
    }

    return LOCAL_BACKEND_URL;
  }

  return backendUrl;
}

export function resolveDataSourceMode(
  env: BackendEnvironment = import.meta.env,
): AppDataSourceMode {
  return normalizeDataSourceMode(env.VITE_DATA_SOURCE);
}

export function isMockDataSourceEnabled(
  env: BackendEnvironment = import.meta.env,
): boolean {
  return resolveDataSourceMode(env) === 'mock';
}

export function resolveRowsEmbedUrl(
  env: Pick<ImportMetaEnv, 'VITE_ROWS_EMBED_URL'> = import.meta.env,
): string | null {
  const normalizedUrl = normalizeUrl(env.VITE_ROWS_EMBED_URL);

  return normalizedUrl ?? null;
}