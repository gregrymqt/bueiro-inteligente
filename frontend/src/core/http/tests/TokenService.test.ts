import { beforeEach, describe, expect, it } from 'vitest';
import { TokenService, normalizeRoles } from '../TokenService';

const createJwt = (payload: Record<string, unknown>): string => {
  const header = Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).toString('base64url');
  const body = Buffer.from(JSON.stringify(payload)).toString('base64url');

  return `${header}.${body}.signature`;
};

describe('TokenService role normalization', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('normaliza o role Admin do backend para admin no frontend', () => {
    const tokenService = new TokenService();

    tokenService.saveToken(
      createJwt({
        sub: 'admin@example.com',
        role: 'Admin',
        exp: Math.floor(Date.now() / 1000) + 3600,
      }),
    );

    expect(tokenService.getRole()).toBe('admin');
  });

  it('usa a role mais privilegiada encontrada na claim roles', () => {
    const tokenService = new TokenService();

    tokenService.saveToken(
      createJwt({
        sub: 'manager@example.com',
        roles: ['User', 'Manager', 'Admin'],
        exp: Math.floor(Date.now() / 1000) + 3600,
      }),
    );

    expect(tokenService.getRole()).toBe('admin');
  });

  it('normaliza a lista de roles do backend para os papéis do frontend', () => {
    expect(normalizeRoles(['Admin', 'Manager', 'User'])).toEqual([
      'admin',
      'manutencao',
      'cidadao',
    ]);
  });
});