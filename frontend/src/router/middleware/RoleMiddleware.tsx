import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { tokenService } from '@/core/http/TokenService';

interface RoleMiddlewareProps {
  allowedRoles: Array<'admin' | 'manutencao' | 'cidadao'>;
}

export const RoleMiddleware: React.FC<RoleMiddlewareProps> = ({ allowedRoles }) => {
  const userRole = tokenService.getRole() as 'admin' | 'manutencao' | 'cidadao' | null;

  // Se não tiver role, ou a role do usuário não estiver entre as permitidas:
  if (!userRole || !allowedRoles.includes(userRole)) {
    // Redireciona para o dashboard caso ele tente acessar uma rota não autorizada
    return <Navigate to="/dashboard" replace />;
  }

  // Se for autorizado, renderiza a rota filha
  return <Outlet />;
};
