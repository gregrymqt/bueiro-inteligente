import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { tokenService } from '@/core/http/TokenService';

export const ProtectedLayout: React.FC = () => {
  const isAuthenticated = !!tokenService.getToken();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Se autenticado, renderiza a rota filha
  return <Outlet />;
};