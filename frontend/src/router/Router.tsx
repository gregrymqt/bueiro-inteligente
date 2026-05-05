import { createBrowserRouter, Navigate } from 'react-router-dom';

// Importando os Guardiões (Middlewares) e as Páginas
import { AuthInterceptor } from '@/core/http/AuthInterceptor';
import { ProtectedLayout } from './middleware/ProtectedLayout';
import { RoleMiddleware } from './middleware/RoleMiddleware';
import Home from '@/pages/Home/Home';
import About from '@/pages/About/About';
import { Login } from '@/pages/Auth/Login';

import { RegisterForm } from '@/feature/auth/components/RegisterForm';

// Importando o novo MainLayout
import { MainLayout } from '@/components/layout/MainLayout/MainLayout';
import { AdminLayout } from '@/components/layout/AdminLayout/AdminLayout';
import { Dashboard } from '@/pages/Dashboard/Dashboard';
import { AuthProvider } from '@/feature/auth/hooks/AuthContext';
import { AdminPlanForm } from '@/feature/plan/components/Form/AdminPlanForm';
import { AdminPlanList } from '@/feature/plan/components/List/AdminPlanList';
import { DashboardHome } from '@/pages/Admin/AdminDashboard';

export const router = createBrowserRouter([
  {
    element: (
      <AuthProvider>
        <AuthInterceptor />
      </AuthProvider>
    ), // Ouve o evento de Unauthorized (Token expirado/inválido)
    children: [
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
      {
        path: '/',
        element: <MainLayout />, // Coloca a Navbar, Sidebar e Footer
        children: [
          {
            index: true, // Isso torna a rota '/' exata e renderiza a Home
            element: <Home />,
          },
          {
            path: 'sobre',
            element: <About />,
          },
          {
            element: <ProtectedLayout />, // Verifica inicialmente se a pessoa está logada
            children: [
              {
                path: 'dashboard',
                element: <Dashboard />,
              },
              // ==========================================
              // 3. EXMPLO DE ROTA COM ROLE (Apenas 'admin' e 'manutencao')
              // ==========================================
              {
                element: <RoleMiddleware allowedRoles={['admin', 'manutencao']} />,
                children: [
                  {
                    path: 'configuracoes',
                    element: <h1>Página de Configurações (Apenas Admin e Manutenção)</h1>,
                  },
                ],
              },
            ],
          },
        ],
      },

      // ==========================================
      // 2. ROTAS PROTEGIDAS (A mágica acontece aqui)
      // ==========================================
      {
        element: <ProtectedLayout />, // Verifica login
        children: [
          {
            element: <RoleMiddleware allowedRoles={['admin']} />, // Apenas Admin[cite: 45]
            children: [
              {
                path: 'admin',
                element: <AdminLayout />, // O Dashboard com Sidebar e Outlet[cite: 45]
                children: [
                  {
                    index: true,
                    element: <Navigate to="home" replace />, // Redireciona /admin para /admin/home[cite: 45]
                  },
                  {
                    path: 'home',
                    element: <DashboardHome />, // Aquela aba Home provisória
                  },
                  {
                    path: 'plans',
                    children: [
                      {
                        index: true, // Rota: /admin/plans
                        element: <AdminPlanList />,
                      },
                      {
                        path: 'new', // Rota: /admin/plans/new
                        element: <AdminPlanForm />,
                      },
                      {
                        path: 'edit/:id', // Rota: /admin/plans/edit/123
                        element: <AdminPlanForm />,
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      },
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
