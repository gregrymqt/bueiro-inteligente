/// <reference types="vite/client" />

// Mapeie aqui todas as suas variáveis de ambiente
interface ImportMetaEnv {
  readonly VITE_BACKEND_URL?: string;
  readonly VITE_ENABLE_RATE_LIMIT?: string;
  readonly VITE_WS_URL?: string;
  readonly VITE_WS_LOCAL?: string;
  readonly VITE_BACKEND_LOCAL?: string;
  readonly VITE_ROWS_EMBED_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}