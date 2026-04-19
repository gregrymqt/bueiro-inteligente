import { describe, expect, it } from 'vitest';
import { DEFAULT_APP_ID, LOCAL_BACKEND_URL, resolveAppId, resolveBackendBaseUrl, resolveUrlMode } from '../environment';

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

describe('resolveAppId', () => {
  it('usa o valor configurado ou cai no default', () => {
    expect(resolveAppId({ VITE_APP_ID: ' custom-app-id ' })).toBe('custom-app-id');
    expect(resolveAppId({ VITE_APP_ID: undefined })).toBe(DEFAULT_APP_ID);
  });
});