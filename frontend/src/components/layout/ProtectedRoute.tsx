import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { tokenService } from '@/core/http/TokenService';

export const ProtectedRoute: React.FC = () => {
  // Verificamos se existe um token válido no nosso serviço isolado
  const isAuthenticated = tokenService.isAuthenticated();

  // Se não estiver autenticado, redireciona para a página de login.
  // O replace={true} evita que o usuário use o botão de "Voltar" do navegador 
  // para tentar acessar a rota protegida novamente.
  if (!isAuthenticated) {
    return <Navigate to="/login" replace={true} />;
  }

  // O <Outlet /> é um componente especial do React Router que diz:
  // "Se passou na verificação, renderize a página filha que está dentro desta rota."
  return <Outlet />;
};