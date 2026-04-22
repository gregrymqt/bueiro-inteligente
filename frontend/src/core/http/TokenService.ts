export type FrontendRole = 'admin' | 'manutencao' | 'cidadao';
export type BackendRole = 'Admin' | 'Manager' | 'User';

// 1. O Contrato (Interface)
export interface ITokenService {
  getToken(): string | null;
  saveToken(token: string): void; // Create / Update
  removeToken(): void; // Delete
  isAuthenticated(): boolean;
  captureTokenFromUrl(urlString?: string): string | null;
}

// Instale: npm install jwt-decode
import { jwtDecode } from "jwt-decode";

export interface DecodedToken {
  sub: string;
  role?: string;
  roles?: string[] | string;
  exp: number;
}

const ROLE_PRIORITY: FrontendRole[] = ['admin', 'manutencao', 'cidadao'];

const ROLE_ALIASES: Record<string, FrontendRole> = {
  admin: 'admin',
  Admin: 'admin',
  MANUTENCAO: 'manutencao',
  manutencao: 'manutencao',
  Manager: 'manutencao',
  manager: 'manutencao',
  cidadao: 'cidadao',
  User: 'cidadao',
  user: 'cidadao',
};

export const normalizeRole = (
  role: string | null | undefined,
): FrontendRole | null => {
  if (!role) {
    return null;
  }

  return ROLE_ALIASES[role.trim()] ?? null;
};

export const normalizeRoles = (
  roles: ReadonlyArray<string | null | undefined> | string | null | undefined,
): FrontendRole[] => {
  const values = Array.isArray(roles) ? roles : roles ? [roles] : [];
  const normalizedRoles = values
    .map((role) => normalizeRole(role))
    .filter((role): role is FrontendRole => role !== null);

  return [...new Set(normalizedRoles)].sort(
    (left, right) => ROLE_PRIORITY.indexOf(left) - ROLE_PRIORITY.indexOf(right),
  );
};

// 2. A Implementação Concreta
export class TokenService implements ITokenService {
  private readonly TOKEN_KEY = "@BueiroInteligente:jwt";

  public getToken(): string | null {
    const token = localStorage.getItem(this.TOKEN_KEY);
    // Verificação básica de expiração para não enviar lixo ao Back-end
    if (token && this.isTokenExpired(token)) {
      this.removeToken();
      return null;
    }
    return token;
  }

  private isTokenExpired(token: string): boolean {
    const { exp } = jwtDecode<DecodedToken>(token);
    return Date.now() >= exp * 1000;
  }

  public getRole(): FrontendRole | null {
    const token = this.getToken();
    if (!token) return null;

    const decodedToken = jwtDecode<DecodedToken>(token);
    const normalizedRoles = normalizeRoles(decodedToken.roles ?? decodedToken.role);

    return normalizedRoles[0] ?? null;
  }

  public saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  public removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  public captureTokenFromUrl(urlString: string = window.location.href): string | null {
    const url = new URL(urlString);
    const token = this.extractTokenFromUrl(url);

    if (!token) {
      return null;
    }

    this.saveToken(token);
    this.cleanTokenFromUrl(url);

    return token;
  }

  private extractTokenFromUrl(url: URL): string | null {
    const queryToken = url.searchParams.get('token');

    if (queryToken) {
      return queryToken;
    }

    if (!url.hash) {
      return null;
    }

    const hashParams = new URLSearchParams(url.hash.replace(/^#/, ''));

    return hashParams.get('token') || hashParams.get('access_token');
  }

  private cleanTokenFromUrl(url: URL): void {
    const searchParams = new URLSearchParams(url.search);
    searchParams.delete('token');
    url.search = searchParams.toString();

    if (url.hash) {
      const hashParams = new URLSearchParams(url.hash.replace(/^#/, ''));
      hashParams.delete('token');
      hashParams.delete('access_token');
      url.hash = hashParams.toString() ? `#${hashParams.toString()}` : '';
    }

    window.history.replaceState({}, document.title, `${url.pathname}${url.search}${url.hash}`);
  }

  public isAuthenticated(): boolean {
    const token = this.getToken();
    // Aqui você poderia adicionar uma biblioteca como 'jwt-decode' no futuro
    // para verificar se o token expirou (exp) antes de retornar true.
    return !!token;
  }
}

// Exportamos uma instância única (Singleton) para uso na aplicação
export const tokenService = new TokenService();
