import React from 'react';
import ReactDOM from 'react-dom/client';
import { RouterProvider } from 'react-router-dom';

// Importando a nossa árvore de rotas recém-criada
import { router } from './router/Router';
import { tokenService } from './core/http/TokenService';

// Se você tinha um index.css ou default global, mantenha a importação aqui
// import './index.css';

// Captura eventuais tokens retornados por OAuth antes de montar a aplicação.
void tokenService.captureTokenFromUrl();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>,
);