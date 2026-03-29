import React, { useState, useMemo } from 'react';
import { Sidebar } from '@/components/layout/Sidebar';
import { NavigationItem } from '@/components/layout/Sidebar/types';

// Importando as nossas Features
import { RealTimeMonitor } from '@/feature/monitoring/components/RealTimeMonitor';
// import { RowsEmbed } from '@/feature/rows/components/RowsEmbed'; // <--- Comentado pois a feature rows ainda não existe

// Importando o estilo do layout da página
import './DashboardLayout.scss';

// SVGs simples para os ícones (Em produção, você pode usar Lucide ou Phosphor Icons)
const ActivityIcon = () => (
  <svg width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" d="M22 12h-4l-3 9L9 3l-3 9H2" />
  </svg>
);

const ChartIcon = () => (
  <svg width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" d="M18 20V10m-6 10V4m-6 16v-4" />
  </svg>
);

const MenuIcon = () => (
  <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h16" />
  </svg>
);

export const Dashboard: React.FC = () => {
  // Estado para controlar a aba ativa (por padrão, a visão em tempo real)
  const [activeTabId, setActiveTabId] = useState<string>('tempo-real');
  
  // Estado para controlar o menu hambúrguer no mobile
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState<boolean>(false);

  // useMemo garante que o array não seja recriado a cada renderização da página
  const navItems: NavigationItem[] = useMemo(() => [
    {
      id: 'tempo-real',
      label: 'Visão Geral (Ao Vivo)',
      icon: <ActivityIcon />,
      component: <RealTimeMonitor bueiroId="bueiro-01" locationName="Bueiro - Terminal Piracicabana" />
    },
    {
      id: 'historico-rows',
      label: 'Análise de Histórico',
      icon: <ChartIcon />,
      // Aqui você colocará o embedUrl gerado pela sua conta do Rows
      // component: <RowsEmbed embedUrl="https://rows.com/embed/sua-planilha-aqui" title="Histórico de Obstrução" />
      component: <div style={{ padding: '20px', textAlign: 'center' }}>Módulo de Histórico (Rows) em breve...</div>
    }
  ], []);

  // Encontra qual componente deve ser renderizado com base no ID ativo
  const activeItem = navItems.find(item => item.id === activeTabId) || navItems[0];

  return (
    <div className="dashboard-layout">
      {/* 1. A nossa Sidebar orientada a dados */}
      <Sidebar 
        items={navItems} 
        activeId={activeTabId} 
        onNavigate={setActiveTabId}
        isOpenMobile={isMobileMenuOpen}
        onCloseMobile={() => setIsMobileMenuOpen(false)}
      />

      {/* 2. Área principal de conteúdo */}
      <div className="dashboard-main">
        
        {/* Topbar exclusiva para o Mobile (escondida no Desktop via SCSS) */}
        <header className="mobile-topbar">
          <button 
            className="mobile-topbar__menu-btn"
            onClick={() => setIsMobileMenuOpen(true)}
            aria-label="Abrir menu de navegação"
          >
            <MenuIcon />
          </button>
          <h2 className="mobile-topbar__title">{activeItem.label}</h2>
        </header>

        {/* O container onde a mágica acontece. O componente ativo é injetado aqui. */}
        <main className="dashboard-content">
          <div className="dashboard-content__header">
            {/* Título visível apenas no Desktop */}
            <h1 className="desktop-title">{activeItem.label}</h1>
          </div>
          
          <div className="dashboard-content__body">
            {activeItem.component}
          </div>
        </main>

      </div>
    </div>
  );
};
