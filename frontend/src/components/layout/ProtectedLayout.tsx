import React, { useState } from 'react';
import { Navigate, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { tokenService } from '@/core/http/TokenService';
import { Sidebar } from './Sidebar/Sidebar';
import { type NavigationItem } from './Sidebar/types';
import { LayoutDashboard, Droplets, History, Settings, Menu } from 'lucide-react'; // Exemplo de ícones
import './ProtectedLayout.scss';

export const ProtectedLayout: React.FC = () => {
  const [isOpenMobile, setIsOpenMobile] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const isAuthenticated = !!tokenService.getToken();

  // Definição dos itens de navegação seguindo sua interface
  const navItems: NavigationItem[] = [
    { id: 'dashboard', label: 'Dashboard', icon: <LayoutDashboard size={20} />, component: null },
    { id: 'monitor', label: 'Monitoramento', icon: <Droplets size={20} />, component: null },
    { id: 'historico', label: 'Histórico', icon: <History size={20} />, component: null },
    { id: 'config', label: 'Configurações', icon: <Settings size={20} />, component: null },
  ];

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Função para navegar e fechar o menu no mobile
  const handleNavigation = (id: string) => {
    navigate(`/${id}`);
    setIsOpenMobile(false);
  };

  return (
    <div className="dashboard-layout">
      {/* Botão de Menu para Mobile (UI/UX) */}
      <header className="mobile-header">
        <button className="mobile-header__menu-btn" onClick={() => setIsOpenMobile(true)}>
          <Menu size={24} />
        </button>
        <span className="mobile-header__logo">Bueiro Inteligente</span>
      </header>

      {/* Chamada correta da Sidebar com todas as Props */}
      <Sidebar 
        items={navItems}
        activeId={location.pathname.replace('/', '') || 'dashboard'}
        onNavigate={handleNavigation}
        isOpenMobile={isOpenMobile}
        onCloseMobile={() => setIsOpenMobile(false)}
      />

      <main className="dashboard-layout__content">
        <Outlet />
      </main>
    </div>
  );
};