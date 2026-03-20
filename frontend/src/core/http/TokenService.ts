// 1. O Contrato (Interface)
export interface ITokenService {
  getToken(): string | null;
  saveToken(token: string): void; // Create / Update
  removeToken(): void; // Delete
  isAuthenticated(): boolean;
}

// Instale: npm install jwt-decode
import { jwtDecode } from "jwt-decode";

export interface DecodedToken {
  sub: string;
  role: "admin" | "manutencao" | "cidadao";
  exp: number;
}

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

  public getRole(): string | null {
    const token = this.getToken();
    if (!token) return null;
    return jwtDecode<DecodedToken>(token).role;
  }

  public saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  public removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
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
