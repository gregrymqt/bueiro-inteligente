import React, { useEffect } from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import { tokenService } from './TokenService';

export const AuthInterceptor: React.FC = () => {
  const navigate = useNavigate();

  useEffect(() => {
    // 1. A função que será executada quando o "grito" for ouvido
    const handleUnauthorized = () => {
      // Garantimos que o token inválido seja deletado
      tokenService.removeToken();
      
      // Chutamos o usuário de volta para o login
      navigate('/login', { replace: true });
    };

    // 2. Inscrevemos o React para escutar o evento nativo do navegador
    window.addEventListener('auth:unauthorized', handleUnauthorized);

    // 3. A Limpeza (Cleanup) - Regra de ouro do useEffect
    // Se este componente for desmontado, paramos de escutar para evitar vazamento de memória
    return () => {
      window.removeEventListener('auth:unauthorized', handleUnauthorized);
    };
  }, [navigate]);

  // Este componente não tem visual próprio.
  // O <Outlet /> renderiza as rotas filhas normalmente.
  return <Outlet />;
};