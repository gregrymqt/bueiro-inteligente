/// <reference types="vite/client" />

// Mapeie aqui todas as suas variáveis de ambiente
interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  // readonly VITE_OUTRA_VARIAVEL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}