import React, { useMemo, useState } from 'react';
import { Sidebar } from '@/components/layout/Sidebar/Sidebar';
import {
  Activity,
  Database,
  MessageSquare,
  History,
  PlusCircle,
  List,
  LineChart,
  RefreshCw
} from 'lucide-react';
import type { NavigationItem } from '@/components/layout/Sidebar/types';

// Feature: Monitoramento
import { RealTimeMonitor } from '@/feature/monitoring/components/RealTimeMonitor';
import { RowsEmbed } from '@/feature/monitoring/components/RowsEmbed';

// Feature: Gestão de Bueiros (Drains)
import { DrainForm } from '@/feature/drain/components/DrainForm';
import { DrainList } from '@/feature/drain/components/DrainList';
import { useDrains } from '@/feature/drain/hooks/useDrains'; 
import type { Drain } from '@/feature/drain/types'; 

// Feature: Feedback
import { FeedbackForm } from '@/feature/feedback/components/FeedbackForm/FeedbackForm';
import { FeedbackList } from '@/feature/feedback/components/FeedbackList/FeedbackList';
import type { Feedback } from '@/feature/feedback/types'; 

import styles from './Dashboard.module.scss';
import { tokenService } from '@/core/http/TokenService';

export const Dashboard: React.FC = () => {
  // --- ESTADOS DE NAVEGAÇÃO E UI ---
  const [activeTabId, setActiveTabId] = useState('live-monitor');
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  // --- ESTADOS DE GESTÃO (CRUD) ---
  const [editingFeedback, setEditingFeedback] = useState<Feedback | null>(null);
  const [editingDrain, setEditingDrain] = useState<Drain | null>(null);

  // 1. Verificar se o utilizador é gestor (admin ou manutenção)
  const userRole = tokenService.getRole();
  const isManager = userRole === 'admin' || userRole === 'manutencao';

  // --- HOOKS DE DADOS ---
  const {
    drains,
    loading: drainsLoading,
    isSaving: drainsSaving,
    refreshDrains,
    createDrain,
    updateDrain,
    deleteDrain
  } = useDrains(); 

  // Estado para refresh manual do feedback (via key re-mount)
  const [feedbackListKey, setFeedbackListKey] = useState(0);

  const handleRefreshFeedback = () => {
    setFeedbackListKey(prev => prev + 1); 
  };

  const dashboardItems = useMemo((): NavigationItem[] => {
    const items: NavigationItem[] = [
      {
        id: 'monitoring',
        label: 'Tempo Real',
        icon: <Activity size={20} />,
        children: [
          { id: 'live-monitor', label: 'Monitor ao Vivo', icon: <Activity size={16} /> },
          { id: 'analysis', label: 'Análise de Histórico', icon: <LineChart size={16} /> },
        ],
      },
    ];

    // Só adiciona Gestão de Bueiros se for Manager/Manutenção
    if (isManager) {
      items.push({
        id: 'drains-management',
        label: 'Gestão de Bueiros',
        icon: <Database size={20} />,
        children: [
          { id: 'drain-list', label: 'Lista de Bueiros', icon: <List size={16} /> },
          { id: 'drain-create', label: 'Cadastrar Novo', icon: <PlusCircle size={16} /> },
        ],
      });
    }

    items.push({
      id: 'feedback-management',
      label: 'Meu Feedback',
      icon: <MessageSquare size={20} />,
      children: [
        { id: 'feedback-send', label: 'Enviar Avaliação', icon: <PlusCircle size={16} /> },
        { id: 'feedback-history', label: 'Histórico de Reviews', icon: <History size={16} /> },
      ],
    });

    return items;
  }, [isManager]);

  const renderContent = () => {
    switch (activeTabId) {
      // --- MONITORAMENTO ---
      case 'live-monitor':
        return <RealTimeMonitor bueiroId="ESP32-FIXO-01" />;
      case 'analysis':
        return <RowsEmbed embedUrl="mock:demo" title="Tendência de Enchentes" />;

      // --- GESTÃO DE BUEIROS ---
      case 'drain-list':
        // Bloqueio de renderização para utilizadores sem permissão
        if (!isManager) {
          return (
            <div className={styles.emptyState}>
              <h2>Acesso Restrito</h2>
              <p>Apenas utilizadores com plano de manutenção ou administradores podem gerir bueiros.</p>
            </div>
          );
        }

        if (editingDrain) {
          return (
            <DrainForm
              initialData={editingDrain}
              isLoading={drainsSaving}
              onCancel={() => setEditingDrain(null)}
              onSubmit={async (data) => {
                const success = await updateDrain(editingDrain.id, data);
                if (success) setEditingDrain(null);
              }}
            />
          ); 
        }
        return (
          <div className={styles.sectionContainer}>
            <header className={styles.sectionHeader}>
              <div>
                <h2 className={styles.sectionTitle}>Bueiros Cadastrados</h2>
                <p className={styles.sectionSubtitle}>Gerencie os dispositivos instalados em campo.</p>
              </div>
              <button onClick={refreshDrains} className={styles.refreshBtn} title="Sincronizar">
                <RefreshCw size={18} className={drainsLoading ? styles.spinning : ''} />
              </button>
            </header>
            <DrainList
              drains={drains}
              loading={drainsLoading}
              isSaving={drainsSaving}
              onEdit={setEditingDrain}
              onDelete={(drain) => deleteDrain(drain.id)}
            />
          </div>
        ); 

      case 'drain-create':
        // Bloqueio de renderização para utilizadores sem permissão
        if (!isManager) {
          return (
            <div className={styles.emptyState}>
              <h2>Acesso Restrito</h2>
              <p>Apenas utilizadores com plano de manutenção ou administradores podem cadastrar bueiros.</p>
            </div>
          );
        }

        return (
          <DrainForm
            isLoading={drainsSaving}
            onSubmit={async (data) => {
              const success = await createDrain(data);
              if (success) setActiveTabId('drain-list');
            }}
          />
        ); 

      // --- FEEDBACK ---
      case 'feedback-send':
        return <FeedbackForm onSuccess={() => setActiveTabId('feedback-history')} />;

      case 'feedback-history':
        if (editingFeedback) {
          return (
            <FeedbackForm
              initialData={editingFeedback}
              onCancel={() => setEditingFeedback(null)}
              onSuccess={() => {
                setEditingFeedback(null);
                handleRefreshFeedback();
              }}
            />
          ); 
        }
        return (
          <div className={styles.sectionContainer}>
            <header className={styles.sectionHeader}>
              <div>
                <h2 className={styles.sectionTitle}>Seu Histórico</h2>
                <p className={styles.sectionSubtitle}>Visualize e gerencie suas avaliações enviadas.</p>
              </div>
              <button onClick={handleRefreshFeedback} className={styles.refreshBtn}>
                <RefreshCw size={18} />
              </button>
            </header>
            <FeedbackList key={feedbackListKey} onEditFeedback={setEditingFeedback} />
          </div>
        ); 

      default:
        return <div className={styles.emptyState}>Selecione uma opção no menu lateral.</div>;
    }
  };

  return (
    <div className={styles.dashboardLayout}>
      <Sidebar
        id="dashboard-sidebar"
        items={dashboardItems}
        activeId={activeTabId}
        onNavigate={(id) => {
          setActiveTabId(id);
          setEditingDrain(null);
          setEditingFeedback(null);
        }}
        isOpenMobile={isSidebarOpen}
        onCloseMobile={() => setIsSidebarOpen(false)}
        showMobileSubheader={true}
        onToggleMobile={() => setIsSidebarOpen(!isSidebarOpen)}
      />

      <main className={styles.dashboardMain}>
        <header className={styles.contentHeader}>
          <h1>Portal do Usuário</h1>
          <p>Gerencie seus dispositivos e acompanhe a telemetria em tempo real.</p>
        </header>

        <div className={styles.contentBody}>
          {renderContent()}
        </div>
      </main>
    </div>
  );
};