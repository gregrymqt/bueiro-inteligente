import { useState, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { AuthService } from '../services/AuthService';
import { tokenService } from '@/core/http/TokenService';
import { AlertService } from '@/core/alert/AlertService';
import type { LoginRequestDTO, UserDTO, RegisterRequestDTO } from '../types';

export const useAuth = () => {
  const [user, setUser] = useState<UserDTO | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    let isActive = true;

    const bootstrapSession = async () => {
      try {
        if (!tokenService.getToken()) {
          return;
        }

        const userData = await AuthService.getMe();

        if (isActive) {
          setUser(userData);
        }
      } catch {
        tokenService.removeToken();

        if (isActive) {
          setUser(null);
        }
      }
    };

    void bootstrapSession();

    return () => {
      isActive = false;
    };
  }, []);

  const register = async (data: RegisterRequestDTO): Promise<boolean> => {
    setLoading(true);
    setError(null);
    try {
      await AuthService.register(data);
      AlertService.success('Cadastro realizado com sucesso!');
      // Redireciona para o login após o cadastro bem sucedido
      navigate('/login', { replace: true });
      return true;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Falha ao realizar o cadastro.';
      setError(message);
      AlertService.error('Erro', message);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const login = async (credentials: LoginRequestDTO) => {
    setLoading(true);
    setError(null);
    try {
      const response = await AuthService.login(credentials);
      tokenService.saveToken(response.access_token);
      
      // Busca os dados do usuário logo após o login para validar roles
      const userData = await AuthService.getMe();
      setUser(userData);

      navigate('/dashboard', { replace: true });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Falha na autenticação.';
      setError(message);
      AlertService.error('Erro', message);
    } finally {
      setLoading(false);
    }
  };

  const logout = useCallback(async () => {
    try {
      await AuthService.logout(); // Avisa o back-end (Blacklist JTI)
    } finally {
      tokenService.removeToken();
      setUser(null);
      setError(null);
      navigate('/login', { replace: true });
    }
  }, [navigate]);

  return { user, register, login, logout, loading, error };
};