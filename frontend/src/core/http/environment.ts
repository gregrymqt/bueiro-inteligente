export type BackendEnvironment = Pick<
  ImportMetaEnv,
  "VITE_BACKEND_URL" | "VITE_BACKEND_LOCAL"
>;

export type AppIdEnvironment = Pick<ImportMetaEnv, "VITE_APP_ID">;

export type SignalRHubEnvironment = Pick<
  ImportMetaEnv,
  "VITE_BACKEND_LOCAL" | "VITE_BACKEND_URL" | "VITE_WS_LOCAL" | "VITE_WS_URL"
>;

export const DEFAULT_APP_ID = "bueiro-inteligente-app-id";

export const LOCAL_BACKEND_URL = "http://localhost:8080";
export const SIGNALR_LOCAL_URL = "http://localhost:8080/realtime/ws";

export type UrlMode = "local" | "tunnel" | "remote";

function isTruthyFlag(value?: string): boolean {
  return value?.trim().toUpperCase() === "TRUE";
}

function normalizeUrl(value?: string): string | null {
  const normalized = value?.trim().replace(/\/+$/, "");

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
    hostname === "localhost" ||
    hostname === "127.0.0.1" ||
    hostname === "::1" ||
    hostname.endsWith(".localhost")
  );
}

function isTunnelHost(hostname: string): boolean {
  return (
    hostname.includes("ngrok") ||
    hostname.includes("grok") ||
    hostname.includes("localtunnel") ||
    hostname.includes("serveo") ||
    hostname.includes("pagekite") ||
    hostname.includes("tunnel")
  );
}

export function resolveUrlMode(urlValue?: string): UrlMode {
  const normalizedUrl = normalizeUrl(urlValue);

  if (!normalizedUrl) {
    return "remote";
  }

  const hostname = getHostname(normalizedUrl);

  if (!hostname) {
    return "remote";
  }

  if (isLocalhostHost(hostname)) {
    return "local";
  }

  if (isTunnelHost(hostname)) {
    return "tunnel";
  }

  return "remote";
}

export function resolveBackendBaseUrl(
  env: BackendEnvironment = import.meta.env,
): string {
  if (isTruthyFlag(env.VITE_BACKEND_LOCAL)) {
    return LOCAL_BACKEND_URL;
  }

  // Caso contrário, tenta ler do .env ou volta para o local como fallback
  return normalizeUrl(env.VITE_BACKEND_URL) ?? LOCAL_BACKEND_URL;
}

export function resolveAppId(env: AppIdEnvironment = import.meta.env): string {
  const configuredAppId = env.VITE_APP_ID?.trim();

  return configuredAppId && configuredAppId.length > 0
    ? configuredAppId
    : DEFAULT_APP_ID;
}

export function resolveSignalRHubUrl(
  env: SignalRHubEnvironment = import.meta.env,
): string {
  // Segue a mesma lógica: se for local, usa a URL de WS local definida no código
  if (isTruthyFlag(env.VITE_WS_LOCAL) || isTruthyFlag(env.VITE_BACKEND_LOCAL)) {
    return SIGNALR_LOCAL_URL;
  }

  const cloudWsUrl = normalizeUrl(env.VITE_WS_URL);
  return cloudWsUrl ?? `${resolveBackendBaseUrl(env)}/realtime/ws`;
}

export function resolveRowsEmbedUrl(
  env: Pick<ImportMetaEnv, "VITE_ROWS_EMBED_URL"> = import.meta.env,
): string | null {
  const normalizedUrl = normalizeUrl(env.VITE_ROWS_EMBED_URL);

  return normalizedUrl ?? null;
}
