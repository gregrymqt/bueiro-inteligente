import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { AuthService } from '../services/AuthService';
import { tokenService } from '@/core/http/TokenService';
import type { LoginRequestDTO, UserDTO } from '../types';

export const useAuth = () => {
  const [user, setUser] = useState<UserDTO | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

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
      setError(err instanceof Error ? err.message : 'Falha na autenticação.');
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
      navigate('/login', { replace: true });
    }
  }, [navigate]);

  return { user, login, logout, loading, error };
};