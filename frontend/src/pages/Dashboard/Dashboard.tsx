import React, { useState } from 'react';
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
import { useDrains } from '@/feature/drain/hooks/useDrains'; //
import type { Drain } from '@/feature/drain/types'; //[cite: 62]

// Feature: Feedback
import { FeedbackForm } from '@/feature/feedback/components/FeedbackForm/FeedbackForm';
import { FeedbackList } from '@/feature/feedback/components/FeedbackList/FeedbackList';
import type { Feedback } from '@/feature/feedback/types'; //[cite: 39]

import styles from './Dashboard.module.scss';

export const Dashboard: React.FC = () => {
  // --- ESTADOS DE NAVEGAÇÃO E UI ---
  const [activeTabId, setActiveTabId] = useState('live-monitor');
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  // --- ESTADOS DE GESTÃO (CRUD) ---
  const [editingFeedback, setEditingFeedback] = useState<Feedback | null>(null);
  const [editingDrain, setEditingDrain] = useState<Drain | null>(null);

  // --- HOOKS DE DADOS ---
  const {
    drains,
    loading: drainsLoading,
    isSaving: drainsSaving,
    refreshDrains,
    createDrain,
    updateDrain,
    deleteDrain
  } = useDrains(); //[cite: 60]

  // Estado para refresh manual do feedback (via key re-mount)
  const [feedbackListKey, setFeedbackListKey] = useState(0);

  const handleRefreshFeedback = () => {
    setFeedbackListKey(prev => prev + 1); //[cite: 63]
  };

  const dashboardItems: NavigationItem[] = [
    {
      id: 'monitoring',
      label: 'Tempo Real',
      icon: <Activity size={20} />,
      children: [
        { id: 'live-monitor', label: 'Monitor ao Vivo', icon: <Activity size={16} /> },
        { id: 'analysis', label: 'Análise de Histórico', icon: <LineChart size={16} /> },
      ],
    },
    {
      id: 'drains-management',
      label: 'Gestão de Bueiros',
      icon: <Database size={20} />,
      children: [
        { id: 'drain-list', label: 'Lista de Bueiros', icon: <List size={16} /> },
        { id: 'drain-create', label: 'Cadastrar Novo', icon: <PlusCircle size={16} /> },
      ],
    },
    {
      id: 'feedback-management',
      label: 'Meu Feedback',
      icon: <MessageSquare size={20} />,
      children: [
        { id: 'feedback-send', label: 'Enviar Avaliação', icon: <PlusCircle size={16} /> },
        { id: 'feedback-history', label: 'Histórico de Reviews', icon: <History size={16} /> },
      ],
    },
  ];

  const renderContent = () => {
    switch (activeTabId) {
      // --- MONITORAMENTO ---
      case 'live-monitor':
        return <RealTimeMonitor bueiroId="ESP32-FIXO-01" />;
      case 'analysis':
        return <RowsEmbed embedUrl="mock:demo" title="Tendência de Enchentes" />;

      // --- GESTÃO DE BUEIROS ---
      case 'drain-list':
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
          ); //[cite: 58]
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
        ); //[cite: 59, 60]

      case 'drain-create':
        return (
          <DrainForm
            isLoading={drainsSaving}
            onSubmit={async (data) => {
              const success = await createDrain(data);
              if (success) setActiveTabId('drain-list');
            }}
          />
        ); //[cite: 58]

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
          ); //[cite: 53]
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
        ); //[cite: 63]

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