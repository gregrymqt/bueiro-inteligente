import React, { useEffect } from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import { tokenService } from './TokenService';
import { useNotifications } from '@/feature/notifications/hooks/useNotifications'; // <-- Importe o hook

export const AuthInterceptor: React.FC = () => {
  const navigate = useNavigate();

  // 🔔 Inicia a escuta do WebSocket para receber notificações de Feedback e Pagamentos
  useNotifications(); 

  useEffect(() => {
    const handleUnauthorized = () => {
      tokenService.removeToken();
      navigate('/login', { replace: true });
    };

    window.addEventListener('auth:unauthorized', handleUnauthorized);

    return () => {
      window.removeEventListener('auth:unauthorized', handleUnauthorized);
    };
  }, [navigate]);

  return <Outlet />;
};