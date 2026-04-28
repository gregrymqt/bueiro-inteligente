import React, { useMemo } from 'react';
import type { NavigationItem } from '@/components/layout/Sidebar/types';
import { resolveRowsEmbedUrl } from '@/core/http/environment';
import { useSearchParams } from 'react-router-dom';

// Importando as nossas Features
import { RealTimeMonitor } from '@/feature/monitoring/components/RealTimeMonitor';

// Importando o estilo do layout da página
import './DashboardLayout.scss';
import { RowsEmbed } from '@/feature/monitoring/components/RowsEmbed';

const USE_DASHBOARD_MOCK = false;

// SVGs simples para os ícones (Em produção, você pode usar Lucide ou Phosphor Icons)
const MonitorIcon = () => (
  <svg width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
    <rect x="3" y="4" width="18" height="12" rx="2" />
    <path strokeLinecap="round" strokeLinejoin="round" d="M8 20h8" />
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 16v4" />
  </svg>
);

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

const findNavigationItem = (
  items: NavigationItem[],
  targetId: string,
): NavigationItem | undefined => {
  for (const item of items) {
    if (item.id === targetId) {
      return item;
    }

    const nestedItem = item.children ? findNavigationItem(item.children, targetId) : undefined;

    if (nestedItem) {
      return nestedItem;
    }
  }

  return undefined;
};

const flattenNavigationItems = (items: NavigationItem[]): NavigationItem[] => {
  return items.flatMap((item) => {
    if (item.children?.length) {
      return flattenNavigationItems(item.children);
    }

    return [item];
  });
};

const findFirstRenderableNavigationItem = (items: NavigationItem[]): NavigationItem | undefined => {
  for (const item of items) {
    if (item.component) {
      return item;
    }

    const nestedItem = item.children ? findFirstRenderableNavigationItem(item.children) : undefined;

    if (nestedItem) {
      return nestedItem;
    }
  }

  return undefined;
};

export const Dashboard: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  // useMemo garante que o array não seja recriado a cada renderização da página
  const rowsEmbedUrl = useMemo(() => {
    if (USE_DASHBOARD_MOCK) {
      return 'mock:rows-dashboard';
    }

    return resolveRowsEmbedUrl() ?? '';
  }, []);

  const navItems: NavigationItem[] = useMemo(() => [
    {
      id: 'monitoramento',
      label: 'Monitoramento',
      icon: <MonitorIcon />,
      children: [
        {
          id: 'tempo-real',
          label: 'Visão Geral (Ao Vivo)',
          icon: <ActivityIcon />,
          component: (
            <RealTimeMonitor bueiroId="bueiro-01" locationName="Bueiro - Terminal Piracicabana" />
          ),
        },
        {
          id: 'historico-rows',
          label: 'Análise de Histórico',
          icon: <ChartIcon />,
          // Aqui você colocará o embedUrl gerado pela sua conta do Rows
          component: <RowsEmbed embedUrl={rowsEmbedUrl} title="Histórico de Obstrução" />,
        },
      ],
    }
  ], [rowsEmbedUrl]);

  const mobileNavItems = useMemo(() => flattenNavigationItems(navItems), [navItems]);
  const requestedTabId = searchParams.get('tab') ?? 'tempo-real';
  const activeTabId = mobileNavItems.some((item) => item.id === requestedTabId)
    ? requestedTabId
    : 'tempo-real';

  const activeItem = findNavigationItem(navItems, activeTabId);
  const activeRenderableItem =
    (activeItem?.component ? activeItem : findFirstRenderableNavigationItem(navItems)) ??
    navItems[0];

  const handleMobileTabChange = (tabId: string) => {
    setSearchParams({ tab: tabId });
  };

  if (!navItems || navItems.length === 0 || !activeRenderableItem?.component) {
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
      <div className="dashboard-main">
        <main className="dashboard-content">
          <div className="dashboard-content__header">
            <h1 className="desktop-title">{activeRenderableItem.label}</h1>
          </div>

          <nav className="mobileTabs" aria-label="Abas do dashboard">
            {mobileNavItems.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => handleMobileTabChange(item.id)}
                className={activeTabId === item.id ? 'tabActive' : 'tab'}
                aria-current={activeTabId === item.id ? 'page' : undefined}
              >
                {item.label}
              </button>
            ))}
          </nav>
          
          <div className="dashboard-content__body">
            {activeRenderableItem.component}
          </div>
        </main>

      </div>
    </div>
  );
};
