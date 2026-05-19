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
  // ==========================================
  // 1. ROTAS PÚBLICAS (Acessíveis sem Login)
  // ==========================================
  {
    path: '/',
    element: (
      <AuthProvider>
        <MainLayout />
      </AuthProvider>
    ),
    children: [
      { index: true, element: <Home /> },
      { path: 'sobre', element: <About /> },
    ],
  },
  {
    path: '/login',
    element: (
      <AuthProvider>
        <Login />
      </AuthProvider>
    ),
  },
  {
    path: '/register',
    element: (
      <AuthProvider>
        <RegisterForm />
      </AuthProvider>
    ),
  },

  // ==========================================
  // 2. ROTAS PROTEGIDAS (Necessitam de Interceptor e Token)
  // ==========================================
  {
    element: (
      <AuthProvider>
        <AuthInterceptor /> 
      </AuthProvider>
    ),
    children: [
      {
        element: <ProtectedLayout />,
        children: [
          // ROTA: Checkout (Qualquer utilizador autenticado)
          {
            path: 'checkout',
            element: <CheckoutLayout />,
            children: [
              {
                index: true,
                element: <CheckoutPage />,
              },
            ],
          },
          // ROTAS: Dashboard (Apenas Admin e Manutenção)
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
                  },
                ],
              },
            ],
          },
          // ROTAS: Admin (Exclusivo para Administradores)
          {
            element: <RoleMiddleware allowedRoles={['admin']} />,
            children: [
              {
                path: 'admin',
                element: <AdminDashboard />, 
                children: [
                  {
                    index: true,
                    element: <Navigate to="home" replace />, 
                  },
                  {
                    path: 'home',
                    element: <DashboardHome />, 
                  },
                  {
                    path: 'plans',
                    children: [
                      {
                        index: true, 
                        element: <AdminPlanList />,
                      },
                      {
                        path: 'new', 
                        element: <AdminPlanForm />,
                      },
                      {
                        path: 'edit/:id', 
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
  // 3. ROTA DE FALLBACK (Página não encontrada)
  // ==========================================
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
]);