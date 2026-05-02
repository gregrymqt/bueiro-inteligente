import React, { useMemo } from 'react';
import type { NavigationItem } from '@/components/layout/Sidebar/types';
import { resolveRowsDashboardUrl, resolveRowsTableUrl } from '@/core/http/environment';
import { useSearchParams } from 'react-router-dom';
import { useState } from 'react';

// Importando as nossas Features
import { RealTimeMonitor } from '@/feature/monitoring/components/RealTimeMonitor';
import { useDrainsList } from '@/feature/monitoring/hooks/useDrainsList';
import { MyDrains } from './MyDrains';
import { StatCardCarousel } from '@/feature/home/components/StatCardCarousel';
import { useHomeCarousel } from '@/feature/home/hooks/useHomeCarousel';

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

const TableIcon = () => (
  <svg width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
    <rect x="3" y="3" width="18" height="18" rx="2" ry="2" />
    <line x1="3" y1="9" x2="21" y2="9" />
    <line x1="3" y1="15" x2="21" y2="15" />
    <line x1="9" y1="3" x2="9" y2="21" />
    <line x1="15" y1="3" x2="15" y2="21" />
  </svg>
);

const SettingsIcon = () => (
  <svg width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 15a3 3 0 100-6 3 3 0 000 6z" />
    <path strokeLinecap="round" strokeLinejoin="round" d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 01-2.83 2.83l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06a1.65 1.65 0 00.33-1.82 1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06a1.65 1.65 0 001.82.33H9a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51h.01a1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06a1.65 1.65 0 00-.33 1.82V9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z" />
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
  const { data: drains, loading: drainsLoading } = useDrainsList();
  const [selectedDrainId, setSelectedDrainId] = useState<string | null>(null);

  const effectiveDrainId = useMemo(() => {
    return selectedDrainId ?? (drains.length > 0 ? drains[0].hardware_id : ''); // ALERTA: Usar hardware_id no fallback
  }, [selectedDrainId, drains]);

  const selectedDrainName = useMemo(() => {
    if (!effectiveDrainId || drains.length === 0) return 'Carregando...';
    // ALERTA: Comparar com d.hardware_id em vez de d.id
    const drain = drains.find(d => d.hardware_id === effectiveDrainId); 
    return drain ? drain.name : 'Bueiro Desconhecido';
  }, [drains, effectiveDrainId]);

  const handleDrainChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedDrainId(event.target.value);
  };

  // useMemo garante que o array não seja recriado a cada renderização da página
  const rowsDashboardUrl = useMemo(() => {
    if (USE_DASHBOARD_MOCK) {
      return 'mock:rows-dashboard';
    }

    return resolveRowsDashboardUrl() ?? '';
  }, []);

  const rowsTableUrl = useMemo(() => {
    if (USE_DASHBOARD_MOCK) {
      return 'mock:rows-table';
    }

    return resolveRowsTableUrl() ?? '';
  }, []);

  const { statItems, loading: statItemsLoading } = useHomeCarousel();

  const navItems: NavigationItem[] = useMemo(() => {
    return [
      {
        id: 'resumo',
        label: 'Resumo Geral',
        icon: <ActivityIcon />,
        component: statItemsLoading ? (
          <div className="dashboard-content__empty">
            <p>Carregando panorama...</p>
          </div>
        ) : (
          <div className="dashboard-content__statcard-wrapper">
            <StatCardCarousel items={statItems} />
          </div>
        ),
      },
      {
        id: 'meus-bueiros',
        label: 'Meus Bueiros',
        icon: <SettingsIcon />,
        component: <MyDrains />,
      },
      {
        id: 'monitoramento',
        label: 'Monitoramento',
        icon: <MonitorIcon />,
        children: [
          {
            id: 'tempo-real',
            label: 'Visão Geral (Ao Vivo)',
            icon: <ActivityIcon />,
            component: effectiveDrainId ? (
              <RealTimeMonitor bueiroId={effectiveDrainId} locationName={selectedDrainName} />
            ) : (
              <div className="dashboard-content__empty">
                <p>Carregando monitoramento...</p>
              </div>
            ),
          },
          {
            id: 'dashboard-rows',
            label: 'Dashboard Analítico',
            icon: <ChartIcon />,
            component: <RowsEmbed embedUrl={rowsDashboardUrl} title="Painel de KPIs" />,
          },
          {
            id: 'tabela-rows',
            label: 'Tabela de Dados',
            icon: <TableIcon />,
            component: <RowsEmbed embedUrl={rowsTableUrl} title="Histórico Completo" />,
          },
        ],
      }
    ]
  }, [rowsDashboardUrl, rowsTableUrl, effectiveDrainId, selectedDrainName, statItems, statItemsLoading]);

  const mobileNavItems = useMemo(() => flattenNavigationItems(navItems), [navItems]);
  const requestedTabId = searchParams.get('tab') ?? 'resumo';
  const activeTabId = mobileNavItems.some((item) => item.id === requestedTabId)
    ? requestedTabId
    : 'resumo';

  const activeItem = findNavigationItem(navItems, activeTabId);
  const activeRenderableItem =
    (activeItem?.component ? activeItem : findFirstRenderableNavigationItem(navItems)) ??
    navItems[0];

  const handleMobileTabChange = (tabId: string) => {
    setSearchParams({ tab: tabId });
  };

  if (!navItems || navItems.length === 0 || !activeRenderableItem?.component) {
    return (
      <div className="dashboard-layout dashboard-layout--centered">
        <div className="dashboard-content__empty">
          <ActivityIcon />
          <p>Nenhum módulo disponível no momento.</p>
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

          <div className="dashboard-content__controls">
            <select
              className="drain-selector"
              value={effectiveDrainId}
              onChange={handleDrainChange}
              disabled={drainsLoading || drains.length === 0}
              aria-label="Selecione um bueiro"
            >
              {drainsLoading ? (
                <option value="">Carregando bueiros...</option>
              ) : drains.length === 0 ? (
                <option value="">Nenhum bueiro disponível</option>
              ) : (
                drains.map(drain => (
                  <option key={drain.id} value={drain.hardware_id}> {/* ALERTA: Usar hardware_id aqui */}
                    {drain.name}
                  </option>
                ))
              )}
            </select>
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
