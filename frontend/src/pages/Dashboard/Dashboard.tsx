import React, { useMemo, useState } from 'react';
import type { NavigationItem } from '@/components/layout/Sidebar/types';
import { resolveRowsDashboardUrl, resolveRowsTableUrl } from '@/core/http/environment';
import { useSearchParams } from 'react-router-dom';

// Importando as nossas Features
import { RealTimeMonitor } from '@/feature/monitoring/components/RealTimeMonitor';
import { useDrainsList } from '@/feature/monitoring/hooks/useDrainsList';
import { RowsEmbed } from '@/feature/monitoring/components/RowsEmbed';

// Importando os componentes de resumo e a nova aba de Meus Bueiros
import { DrainManagement } from './DrainManagement';

// Importando o estilo do layout da página
import './DashboardLayout.scss';

const USE_DASHBOARD_MOCK = false;

// SVGs simples para os ícones
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
    <path strokeLinecap="round" strokeLinejoin="round" d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 01-2.83 2.83l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09a1.65 1.65 0 00-1-1.51 1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 11-2.83-2.83l.06-.06a1.65 1.65 0 00.33-1.82 1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09a1.65 1.65 0 001.51-1 1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 112.83-2.83l.06.06a1.65 1.65 0 001.82.33H9a1.65 1.65 0 001-1.51V3a2 2 0 114 0v.09a1.65 1.65 0 001 1.51h.22a1.65 1.65 0 001.82-.33l.06-.06a2 2 0 112.83 2.83l-.06.06a1.65 1.65 0 00-.33 1.82V9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z" />
  </svg>
);

const HomeIcon = () => (
  <svg width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z" />
    <polyline strokeLinecap="round" strokeLinejoin="round" points="9 22 9 12 15 12 15 22" />
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

const ResumoDrains: React.FC<{ drainsCount: number, activeDrainsCount: number }> = ({ drainsCount, activeDrainsCount }) => {
  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1rem', marginBottom: '2rem' }}>
      <div style={{ padding: '1.5rem', backgroundColor: '#fff', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', border: '1px solid #e5e7eb' }}>
        <h3 style={{ fontSize: '0.875rem', fontWeight: 600, color: '#6b7280', margin: '0 0 0.5rem 0', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Total de Bueiros</h3>
        <p style={{ fontSize: '2rem', fontWeight: 700, color: '#111827', margin: 0 }}>{drainsCount}</p>
      </div>
      <div style={{ padding: '1.5rem', backgroundColor: '#fff', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', border: '1px solid #e5e7eb' }}>
        <h3 style={{ fontSize: '0.875rem', fontWeight: 600, color: '#6b7280', margin: '0 0 0.5rem 0', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Bueiros Ativos</h3>
        <p style={{ fontSize: '2rem', fontWeight: 700, color: '#10b981', margin: 0 }}>{activeDrainsCount}</p>
      </div>
    </div>
  );
};


export const Dashboard: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const { data: drains, loading: drainsLoading } = useDrainsList();
  const [selectedDrainId, setSelectedDrainId] = useState<string | null>(null);

  const effectiveDrainId = useMemo(() => {
    return selectedDrainId ?? (drains.length > 0 ? drains[0].hardware_id : '');
  }, [selectedDrainId, drains]);

  const selectedDrainName = useMemo(() => {
    if (!effectiveDrainId || drains.length === 0) return 'Carregando...';
    const drain = drains.find(d => d.hardware_id === effectiveDrainId); 
    return drain ? drain.name : 'Bueiro Desconhecido';
  }, [drains, effectiveDrainId]);

  const handleDrainChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedDrainId(event.target.value);
  };

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

  const activeDrainsCount = useMemo(() => drains.filter(d => d.is_active).length, [drains]);

  const navItems: NavigationItem[] = useMemo(() => {
    return [
      {
        id: 'overview',
        label: 'Resumo Geral',
        icon: <HomeIcon />,
        component: (
          <div>
            <ResumoDrains drainsCount={drains.length} activeDrainsCount={activeDrainsCount} />
            {effectiveDrainId ? (
              <>
                 <h2 style={{ fontSize: '1.25rem', fontWeight: 600, color: '#374151', marginBottom: '1rem' }}>Monitoramento Rápido</h2>
                 <RealTimeMonitor bueiroId={effectiveDrainId} locationName={selectedDrainName} />
              </>
            ) : (
              <div style={{ textAlign: 'center', padding: '2rem', color: '#6b7280', backgroundColor: '#fff', borderRadius: '8px', border: '1px dashed #d1d5db' }}>
                <p>Nenhum bueiro disponível para monitoramento no momento.</p>
                <p style={{ fontSize: '0.875rem', marginTop: '0.5rem' }}>Cadastre um novo bueiro na aba "Meus Bueiros".</p>
              </div>
            )}
          </div>
        )
      },
      {
        id: 'monitoramento',
        label: 'Monitoramento Detalhado',
        icon: <MonitorIcon />,
        children: [
          {
            id: 'tempo-real',
            label: 'Visão Geral (Ao Vivo)',
            icon: <ActivityIcon />,
            component: effectiveDrainId ? (
              <RealTimeMonitor bueiroId={effectiveDrainId} locationName={selectedDrainName} />
            ) : (
              <div style={{ textAlign: 'center', padding: '2rem', color: '#6b7280' }}>
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
      },
      {
        id: 'gerenciamento',
        label: 'Meus Bueiros',
        icon: <SettingsIcon />,
        component: <DrainManagement />
      }
    ]
  }, [rowsDashboardUrl, rowsTableUrl, effectiveDrainId, selectedDrainName, drains.length, activeDrainsCount]);

  const mobileNavItems = useMemo(() => flattenNavigationItems(navItems), [navItems]);
  const requestedTabId = searchParams.get('tab') ?? 'overview';
  const activeTabId = mobileNavItems.some((item) => item.id === requestedTabId)
    ? requestedTabId
    : 'overview';

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

          {(activeTabId === 'tempo-real' || activeTabId === 'overview') && (
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
                    <option key={drain.id} value={drain.hardware_id}>
                      {drain.name}
                    </option>
                  ))
                )}
              </select>
            </div>
          )}

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
