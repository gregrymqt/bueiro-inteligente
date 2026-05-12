import React, { useState } from 'react';
import { Home, Layers, List, PlusSquare, LayoutTemplate } from 'lucide-react';
import { Sidebar } from '@/components/layout/Sidebar/Sidebar';
import type { NavigationItem } from '@/components/layout/Sidebar/types';
import type { PricingPlan } from '@/feature/plan/types';
import styles from './AdminDashboard.module.scss';

// Importações da Feature de Planos
import { AdminPlanList } from '@/feature/plan/components/List/AdminPlanList';
import { AdminPlanForm } from '@/feature/plan/components/Form/AdminPlanForm';

// Importação do Orquestrador de CRUD da Home (Feature AdminHome)
import { HomeAdminManager } from '@/feature/home/admin/components/HomeAdminManager';

// Componente da Visão Geral
export const DashboardHome = () => (
  <div className={styles.homeTab}>
    <h2>Visão Geral</h2>
    <p>Bem-vindo ao painel de administração do Bueiro Inteligente. Aqui você poderá acompanhar as métricas e gerenciar o sistema.</p>
  </div>
);

export const AdminDashboard: React.FC = () => {
  const [activeId, setActiveId] = useState('overview');
  const [isOpenMobile, setIsOpenMobile] = useState(false);
  const [editingPlan, setEditingPlan] = useState<PricingPlan | undefined>(undefined);

  // Configuração dos itens da Sidebar
  const navigationItems: NavigationItem[] = [
    { id: 'overview', label: 'Visão Geral', icon: <Home size={20} /> },
    {
      id: 'gestao-home',
      label: 'Conteúdo Home',
      icon: <LayoutTemplate size={20} />
    },
    {
      id: 'planos',
      label: 'Planos',
      icon: <Layers size={20} />,
      children: [
        { id: 'planos-list', label: 'Lista de Planos', icon: <List size={18} /> },
        { id: 'planos-form', label: 'Novo Plano', icon: <PlusSquare size={18} /> }
      ]
    }
  ];

  const handleNavigate = (id: string) => {
    setActiveId(id);

    // Limpa o plano em edição caso o usuário saia da tela de formulário
    if (id !== 'planos-form') {
      setEditingPlan(undefined);
    }
  };

  // Handlers de Sucesso e Edição para a Feature de Planos
  const handleEditPlan = (plan: PricingPlan) => {
    setEditingPlan(plan);
    setActiveId('planos-form');
  };

  const handleFormSuccess = () => {
    setActiveId('planos-list');
    setEditingPlan(undefined);
  };

  return (
    <div className={styles.dashboardLayout}>
      <Sidebar
        id="admin-sidebar"
        items={navigationItems}
        activeId={activeId}
        onNavigate={handleNavigate}
        isOpenMobile={isOpenMobile}
        onCloseMobile={() => setIsOpenMobile(false)}
        onToggleMobile={() => setIsOpenMobile(prev => !prev)}
        showMobileSubheader={true}
      />

      <main className={styles.mainContent}>
        {/* Renderização Condicional das Abas baseada no estado local */}
        {activeId === 'overview' && <DashboardHome />}

        {/* --- CRUD DA HOME (Totalmente encapsulado pelo Manager) --- */}
        {activeId === 'gestao-home' && <HomeAdminManager />}

        {/* --- CRUD DE PLANOS --- */}
        {activeId === 'planos-list' && (
          <AdminPlanList onEdit={handleEditPlan} />
        )}

        {activeId === 'planos-form' && (
          <AdminPlanForm
            initialData={editingPlan}
            onSuccess={handleFormSuccess}
          />
        )}
      </main>
    </div>
  );
};