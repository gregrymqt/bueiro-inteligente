// 1. O Contrato (Interface)
export interface ITokenService {
  getToken(): string | null;
  saveToken(token: string): void; // Create / Update
  removeToken(): void;            // Delete
  isAuthenticated(): boolean;
}

// 2. A Implementação Concreta
export class TokenService implements ITokenService {
  private readonly TOKEN_KEY = '@BueiroInteligente:jwt';

  public getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
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