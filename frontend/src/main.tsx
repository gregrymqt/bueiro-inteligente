import React from 'react';
import ReactDOM from 'react-dom/client';
import { RouterProvider } from 'react-router-dom';

// Importando a nossa árvore de rotas recém-criada
import { router } from './Router';

// Se você tinha um index.css ou default global, mantenha a importação aqui
// import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>,
);