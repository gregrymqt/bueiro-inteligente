import { createBrowserRouter, Navigate, Outlet } from 'react-router-dom';

// Importando os Guardiões e as Páginas
import { ProtectedRoute } from '@/components/layout/ProtectedRoute'; // Assumindo que este seja ou invoque o ProtectedLayout
import { AuthInterceptor } from '@/core/http/AuthInterceptor';
import { Dashboard } from '@/pages/Dashboard';
import { Login } from './pages/Auth/Login'; 

// Importando os novos componentes de Layout Global
import { Navbar } from '@/components/layout/Navbar/Navbar';
import { Footer } from '@/components/layout/Footer/Footer';

// Layout raiz para englobar todas as páginas com Navbar e Footer
const RootLayout = () => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <Navbar />
      <main style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        <Outlet />
      </main>
      <Footer />
    </div>
  );
};

export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      // ==========================================
      // 1. ROTAS PÚBLICAS (Sem verificação de JWT)
      // ==========================================
      {
        path: '/login',
        element: <Login />,
      },
      
      // ==========================================
      // 2. ROTAS PROTEGIDAS (A mágica acontece aqui)
      // ==========================================
      {
        element: <AuthInterceptor />,
        children: [
          {
            path: '/',
            // Se você renomeou ProtectedRoute para ProtectedLayout, basta ajustar aqui. 
            // O Guardião renderiza a Outlet para as filhas.
            element: <ProtectedRoute />, 
            children: [
              {
                path: '/',
                element: <Navigate to="/dashboard" replace />,
              },
              {
                path: '/dashboard',
                element: <Dashboard />,
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
        element: <Navigate to="/dashboard" replace />, 
      }
    ]
  }
]);