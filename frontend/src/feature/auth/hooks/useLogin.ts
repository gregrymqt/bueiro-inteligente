import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AuthService } from '../services/AuthService';
import { tokenService } from '@/core/http/TokenService';
import type { LoginRequestDTO } from '../types';

export const useLogin = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  // Hook do React Router DOM para navegação programática
  const navigate = useNavigate();

  const login = async (credentials: LoginRequestDTO) => {
    setLoading(true);
    setError(null);

    try {
      // 1. Chama a API
      const response = await AuthService.login(credentials);

      // 2. Salva o JWT no localStorage através do nosso serviço isolado
      tokenService.saveToken(response.access_token);

      // 3. Redireciona o usuário para o Dashboard.
      // O replace: true substitui a rota atual no histórico do navegador.
      // Isso impede que o usuário clique em "Voltar" e caia na tela de login de novo.
      navigate('/dashboard', { replace: true });

    } catch (err) {
      // O erro já vem tratado do nosso ApiClient (com o detail do FastAPI)
      setError(err instanceof Error ? err.message : 'Falha na autenticação. Verifique suas credenciais.');
    } finally {
      setLoading(false);
    }
  };

  return { login, loading, error };
};