import { createBrowserRouter, Navigate } from 'react-router-dom';

// Importando os Guardiões (Middlewares) e as Páginas
import { AuthInterceptor } from '@/core/http/AuthInterceptor';
import { ProtectedLayout } from './middleware/ProtectedLayout';
import { RoleMiddleware } from './middleware/RoleMiddleware';
import { Dashboard } from '@/pages/Dashboard/Dashboard';
import Home from '@/pages/Home/Home';
import { Login } from '@/pages/Auth/Login'; 

import { RegisterForm } from '@/feature/auth/components/RegisterForm';

// Importando o novo MainLayout
import { MainLayout } from '@/components/layout/MainLayout/MainLayout';

export const router = createBrowserRouter([
  // ==========================================
  // 1. ROTAS PÚBLICAS (Sem verificação de JWT)
  // ==========================================
  {
    path: '/login',
    element: <Login />,
  },
  {
    path: '/register',
    element: <RegisterForm />,
  },
  
  // ==========================================
  // 2. ROTAS PROTEGIDAS (A mágica acontece aqui)
  // ==========================================
  {
    element: <AuthInterceptor />, // Ouve o evento de Unauthorized (Token expirado/inválido)
    children: [
      {
        element: <ProtectedLayout />, // Verifica inicialmente se a pessoa está logada
        children: [
          {
            path: '/',
            element: <MainLayout />, // Coloca a Navbar, Sidebar e Footer
            children: [
              {
                index: true, // Isso torna a rota '/' exata e renderiza a Home
                element: <Home />,
              },
              {
                path: '/dashboard',
                element: <Dashboard />,
              },
              // ==========================================
              // 3. EXMPLO DE ROTA COM ROLE (Apenas 'admin' e 'manutencao')
              // ==========================================
              {
                element: <RoleMiddleware allowedRoles={['admin', 'manutencao']} />,
                children: [
                  {
                    path: '/configuracoes',
                    element: <h1>Página de Configurações (Apenas Admin e Manutenção)</h1>,
                  }
                ]
              }
            ],
          },
        ],
      }
    ],
  },
  
  // ==========================================
  // ROTA DE FALLBACK (Página não encontrada)
  // ==========================================
  {
    path: '*',
    element: <Navigate to="/dashboard" replace />, 
  }
]);