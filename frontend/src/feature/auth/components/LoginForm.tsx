import React, { useState } from 'react';
import './LoginForm.scss';
import { useAuth } from '../hooks/useAuth';

export const LoginForm: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { login, loading, error } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await login({ email, password });
  };

  return (
    <div className="login-container">
      <form className="login-form" onSubmit={handleSubmit}>
        <h1 className="login-form__title">Acesso ao Sistema</h1>
        <p className="login-form__subtitle">Monitore o ecossistema de bueiros em tempo real.</p>

        {error && <div className="login-form__error">{error}</div>}

        <div className="form-group">
          <label htmlFor="email">E-mail</label>
          <input
            id="email"
            type="email"
            placeholder="seu@email.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="password">Senha</label>
          <input
            id="password"
            type="password"
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        <button type="submit" className="login-form__button" disabled={loading}>
          {loading ? 'Autenticando...' : 'Entrar'}
        </button>
      </form>
    </div>
  );
};