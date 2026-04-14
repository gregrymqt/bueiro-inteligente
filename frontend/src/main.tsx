import React from 'react';
import ReactDOM from 'react-dom/client';

import App from './App';
import { tokenService } from './core/http/TokenService';

// Se você tinha um index.css ou default global, mantenha a importação aqui
// import './index.css';

// Captura eventuais tokens retornados por OAuth antes de montar a aplicação.
void tokenService.captureTokenFromUrl();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);