import { describe, expect, it } from 'vitest';
import { LOCAL_BACKEND_URL, isMockDataSourceEnabled, resolveBackendBaseUrl, resolveDataSourceMode, resolveUrlMode } from '../environment';

describe('resolveBackendBaseUrl', () => {
  it('prioriza o túnel público quando configurado', () => {
    expect(
      resolveBackendBaseUrl({
        VITE_BACKEND_URL: 'https://backend-tunnel.example.ngrok-free.app/',
      }),
    ).toBe('https://backend-tunnel.example.ngrok-free.app');
  });

  it('usa o backend local quando a flag local está ativa e não existe URL', () => {
    expect(
      resolveBackendBaseUrl({
        VITE_BACKEND_LOCAL: 'TRUE',
      }),
    ).toBe(LOCAL_BACKEND_URL);
  });

  it('usa a URL de produção quando a URL está configurada', () => {
    expect(
      resolveBackendBaseUrl({
        VITE_BACKEND_LOCAL: 'TRUE',
        VITE_BACKEND_URL: 'https://production.example.com/',
      }),
    ).toBe('https://production.example.com');
  });
});

describe('resolveUrlMode', () => {
  it('detecta localhost', () => {
    expect(resolveUrlMode('http://localhost:8080')).toBe('local');
    expect(resolveUrlMode('http://127.0.0.1:8000')).toBe('local');
  });

  it('detecta urls de tunel', () => {
    expect(resolveUrlMode('https://abc.ngrok-free.app')).toBe('tunnel');
    expect(resolveUrlMode('https://example.grok-tunnel.app')).toBe('tunnel');
  });

  it('classifica urls remotas como remote', () => {
    expect(resolveUrlMode('https://api.example.com')).toBe('remote');
  });
});

describe('resolveDataSourceMode', () => {
  it('usa mock por padrão quando a flag nao esta definida', () => {
    expect(resolveDataSourceMode({})).toBe('mock');
    expect(isMockDataSourceEnabled({})).toBe(true);
  });

  it('ativa backend quando a flag recebe backend', () => {
    expect(
      resolveDataSourceMode({
        VITE_DATA_SOURCE: 'backend',
      }),
    ).toBe('backend');

    expect(
      isMockDataSourceEnabled({
        VITE_DATA_SOURCE: 'backend',
      }),
    ).toBe(false);
  });
});