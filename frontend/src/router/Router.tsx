import { createBrowserRouter, Navigate } from 'react-router-dom';

// Importando os Guardiões (Middlewares) e as Páginas
import { AuthInterceptor } from '@/core/http/AuthInterceptor';
import { ProtectedLayout } from './middleware/ProtectedLayout';
import { RoleMiddleware } from './middleware/RoleMiddleware';
import Home from '@/pages/Home/Home';
import About from '@/pages/About/About';
import { Login } from '@/pages/Auth/Login';

import { RegisterForm } from '@/feature/auth/components/RegisterForm';

// Importando o novo MainLayout e outros layouts
import { MainLayout } from '@/components/layout/MainLayout/MainLayout';
import { DashboardLayout } from '@/components/layout/DashboardLayout/DashboardLayout';
import { CheckoutLayout } from '@/components/layout/CheckoutLayout/CheckoutLayout';
import { Dashboard } from '@/pages/Dashboard/Dashboard';
import { AuthProvider } from '@/feature/auth/hooks/AuthContext';
import { AdminPlanForm } from '@/feature/plan/components/Form/AdminPlanForm';
import { AdminPlanList } from '@/feature/plan/components/List/AdminPlanList';
import { AdminDashboard, DashboardHome } from '@/pages/Admin/AdminDashboard';
import { CheckoutPage } from '@/pages/Checkout/CheckoutPage';

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
        ],
      },
      
      // ==========================================
      // ROTAS PROTEGIDAS (Sem Main Layout)
      // ==========================================
      {
        element: <ProtectedLayout />,
        children: [
          {
            path: 'checkout',
            element: <CheckoutLayout />,
            children: [
              {
                index: true,
                element: <CheckoutPage />,
              },
            ]
          },
          {
            element: <RoleMiddleware allowedRoles={['admin', 'manutencao']} />,
            children: [
              {
                path: 'dashboard',
                element: <DashboardLayout />,
                children: [
                  {
                    index: true,
                    element: <Dashboard />, 
                  }
                ]
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
                element: <AdminDashboard />, // O Dashboard com Sidebar e Outlet[cite: 45]
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
