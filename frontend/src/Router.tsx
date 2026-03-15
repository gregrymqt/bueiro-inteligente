import { createBrowserRouter, Navigate } from 'react-router-dom';

// Importando os Guardiões e as Páginas
import { ProtectedRoute } from '@/components/layout/ProtectedRoute';
import { AuthInterceptor } from '@/core/http/AuthInterceptor';
import { Dashboard } from '@/pages/Dashboard';
import { Login } from './pages/Auth/Login'; // Mantive o seu caminho correto

export const router = createBrowserRouter([
  // ==========================================
  // 1. ROTAS PÚBLICAS (Sem verificação de JWT)
  // ==========================================
  {
    path: '/login',
    element: <Login />, // O componente real substituiu o placeholder e ficou de fora do Guardião
  },
  
  // ==========================================
  // 2. ROTAS PROTEGIDAS (A mágica acontece aqui)
  // ==========================================
  // O AuthInterceptor fica no topo para ouvir se o token expirou (Erro 401)
  {
    element: <AuthInterceptor />,
    children: [
      {
        path: '/',
        element: <ProtectedRoute />, // O seu Guardião continua firme e forte como PAI!
        children: [
          {
            path: '/',
            element: <Navigate to="/dashboard" replace />, // Redireciona a raiz direto pro Dashboard
          },
          {
            path: '/dashboard',
            element: <Dashboard />,
          },
          // Futuras rotas do Bueiro Inteligente (ex: mapa, relatórios) entram aqui!
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
]);