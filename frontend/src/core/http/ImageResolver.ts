// src/core/utils/ImageResolver.ts
import { resolveBackendBaseUrl } from '../http/environment'; // Ajuste o caminho relativo de importação se necessário

/**
 * Utilitário para normalizar caminhos de imagem vindos do Backend.
 * Ele diferencia automaticamente URLs do Supabase (externas) de caminhos locais.
 */
export class ImageResolver {
  public static resolve(path: string | null | undefined): string {
    if (!path) {
      // Retorna um placeholder ou string vazia se não houver caminho
      return '/assets/images/placeholder-drain.png'; 
    }

    // 1. Se já for uma URL absoluta (Supabase ou CDN externa completa)
    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }

    // 2. Limpa barras duplicadas no início do caminho
    const cleanPath = path.startsWith('/') ? path : `/${path}`;
    
    // 3. Obtém a URL base dinamicamente. 
    // Se VITE_BACKEND_LOCAL=TRUE, baseUrl será "" (string vazia), mantendo a requisição na porta 5173 via Proxy.
    // Se estiver em produção remota, baseUrl conterá o domínio completo da nuvem.
    const baseUrl = resolveBackendBaseUrl();
    
    return `${baseUrl}${cleanPath}`;
  }
}