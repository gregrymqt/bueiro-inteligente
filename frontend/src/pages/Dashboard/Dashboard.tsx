import React, { useState, useMemo } from 'react';
import { Sidebar } from '@/components/layout/Sidebar/Sidebar';
import type { NavigationItem } from '@/components/layout/Sidebar/types';
import { isMockDataSourceEnabled, resolveRowsEmbedUrl } from '@/core/http/environment';

// Importando as nossas Features
import { RealTimeMonitor } from '@/feature/monitoring/components/RealTimeMonitor';

// Importando o estilo do layout da página
import styles from './DashboardLayout.module.scss';
import './DashboardLayout.scss';
import { RowsEmbed } from '@/feature/monitoring/components/RowsEmbed';

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

export const Dashboard: React.FC = () => {
  // Estado para controlar a aba ativa (por padrão, a visão em tempo real)
  const [activeTabId, setActiveTabId] = useState<string>('tempo-real');

  // useMemo garante que o array não seja recriado a cada renderização da página
  const rowsEmbedUrl = useMemo(() => {
    if (isMockDataSourceEnabled()) {
      return 'mock:rows-dashboard';
    }

    return resolveRowsEmbedUrl() ?? '';
  }, []);

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
      component: <RowsEmbed embedUrl={rowsEmbedUrl} title="Histórico de Obstrução" />
    }
  ], [rowsEmbedUrl]);

  // Encontra qual componente deve ser renderizado com base no ID ativo
  const activeItem = navItems.find(item => item.id === activeTabId) || navItems[0];

  if (!navItems || navItems.length === 0 || !activeItem) {
    return (
      <div className="dashboard-layout" style={{ justifyContent: 'center', alignItems: 'center' }}>
        <div style={{ textAlign: 'center', padding: '2rem', color: '#6b7280', display: 'flex', flexDirection: 'column', gap: '1rem', alignItems: 'center' }}>
          <ActivityIcon />
          <p style={{ margin: 0, fontSize: '1.1rem' }}>Nenhum módulo disponível no momento.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-layout">
      <div className={styles.dashboardSidebarContainer}>
        <Sidebar
          id="dashboard-sidebar"
          items={navItems}
          activeId={activeTabId}
          onNavigate={setActiveTabId}
          isOpenMobile={false}
          onCloseMobile={() => {}}
        />
      </div>

      <div className="dashboard-main">
        <main className="dashboard-content">
          <div className="dashboard-content__header">
            <h1 className="desktop-title">{activeItem.label}</h1>
          </div>

          <nav className="mobileTabs" aria-label="Abas do dashboard">
            {navItems.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => setActiveTabId(item.id)}
                className={activeTabId === item.id ? 'tabActive' : 'tab'}
                aria-current={activeTabId === item.id ? 'page' : undefined}
              >
                {item.label}
              </button>
            ))}
          </nav>
          
          <div className="dashboard-content__body">
            {activeItem.component}
          </div>
        </main>

      </div>
    </div>
  );
};
