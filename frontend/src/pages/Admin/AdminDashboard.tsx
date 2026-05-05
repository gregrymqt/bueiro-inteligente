import React, { useState } from 'react';
import { Home, Layers, List, PlusSquare } from 'lucide-react';
import { Sidebar } from '@/components/layout/Sidebar/Sidebar'; // Ajuste o path se necessário
import type { NavigationItem } from '@/components/layout/Sidebar/types'; // Ajuste o path
import type { PricingPlan } from '@/feature/plan/types';
import styles from './AdminDashboard.module.scss';
import { AdminPlanList } from '@/feature/plan/components/List/AdminPlanList';
import { AdminPlanForm } from '@/feature/plan/components/Form/AdminPlanForm';

// Componente provisório para a Home
export const DashboardHome = () => (
  <div className={styles.homeTab}>
    <h2>Visão Geral</h2>
    <p>Bem-vindo ao painel de administração do Bueiro Inteligente. Aqui você poderá acompanhar as métricas e gerenciar o sistema.</p>
  </div>
);

export const AdminDashboard: React.FC = () => {
  const [activeId, setActiveId] = useState('home');
  const [isOpenMobile, setIsOpenMobile] = useState(false);
  const [editingPlan, setEditingPlan] = useState<PricingPlan | undefined>(undefined);

  // Configuração dos itens da Sidebar
  const navigationItems: NavigationItem[] = [
    { id: 'home', label: 'Home', icon: <Home size={20} /> },
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
    
    // Se saiu da aba de formulário, limpamos o estado de edição para 
    // que o próximo clique em "Novo Plano" venha vazio.
    if (id !== 'planos-form') {
      setEditingPlan(undefined);
    }
  };

  const handleEditPlan = (plan: PricingPlan) => {
    setEditingPlan(plan);
    setActiveId('planos-form'); // Muda visualmente para a aba do formulário
  };

  const handleFormSuccess = () => {
    // Após salvar com sucesso, volta para a lista e limpa o formulário
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
        showMobileSubheader={true} // Ativa o header no mobile automaticamente[cite: 37]
      />

      <main className={styles.mainContent}>
        {/* Renderização Condicional das Abas */}
        {activeId === 'home' && <DashboardHome />}
        
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