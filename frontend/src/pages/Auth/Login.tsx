import React, { useState } from 'react';
import { useLogin } from '@/features/auth/hooks/useLogin';
import './Login.scss';

export const Login: React.FC = () => {
  // Estados locais para os inputs controlados
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');

  // Trazemos as armas pesadas do nosso Hook
  const { login, loading, error } = useLogin();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault(); // Evita que a página recarregue ao dar Enter

    // Validação básica de front-end antes de bater na API
    if (!email || !password) return;

    // Dispara a requisição
    await login({ email, password });
  };

  return (
    <div className="login-layout">
      <main className="login-card">
        <header className="login-card__header">
          <h1 className="login-card__title">Bueiro Inteligente</h1>
          <p className="login-card__subtitle">Acesse o painel de monitoramento IoT</p>
        </header>

        <form className="login-form" onSubmit={handleSubmit}>
          {/* Exibição de Erro Centralizada */}
          {error && (
            <div className="login-form__error" role="alert">
              {error}
            </div>
          )}

          <div className="form-group">
            <label htmlFor="email">E-mail</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="admin@praiagrande.sp.gov.br"
              required
              disabled={loading} // Bloqueia digitação durante o loading
              autoComplete="email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Senha</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
              disabled={loading}
            />
          </div>

          <button 
            type="submit" 
            className={`btn-primary ${loading ? 'btn-primary--loading' : ''}`}
            disabled={loading || !email || !password}
          >
            {loading ? 'Autenticando...' : 'Entrar no Sistema'}
          </button>
        </form>
      </main>
    </div>
  );
};