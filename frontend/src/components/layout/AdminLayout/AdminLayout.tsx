// AdminLayout.tsx
import React, { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Home, Layers, List, PlusSquare } from 'lucide-react';
import { Sidebar } from '@/components/layout/Sidebar/Sidebar'; //[cite: 42]
import styles from './AdminDashboard.module.scss';

export const AdminLayout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [isOpenMobile, setIsOpenMobile] = useState(false);

  // Mapeamos o path atual para o activeId da Sidebar
  const activeId = location.pathname.split('/').pop() || 'home';

  const navigationItems = [
    { id: 'home', label: 'Home', icon: <Home size={20} /> },
    {
      id: 'plans',
      label: 'Planos',
      icon: <Layers size={20} />,
      children: [
        { id: 'plans-list', label: 'Lista de Planos', icon: <List size={18} />, path: '/admin/plans' },
        { id: 'plans-new', label: 'Novo Plano', icon: <PlusSquare size={18} />, path: '/admin/plans/new' }
      ]
    }
  ];

  const handleNavigate = (id: string) => {
    // Encontra o item para saber o path real
    if (id === 'home') navigate('/admin/home');
    if (id === 'plans-list') navigate('/admin/plans');
    if (id === 'plans-new') navigate('/admin/plans/new');
  };

  return (
    <div className={styles.dashboardLayout}>
      <Sidebar
        id="admin-sidebar"
        items={navigationItems}
        activeId={activeId}
        onNavigate={handleNavigate} //[cite: 42]
        isOpenMobile={isOpenMobile}
        onCloseMobile={() => setIsOpenMobile(false)}
        onToggleMobile={() => setIsOpenMobile(prev => !prev)}
        showMobileSubheader={true} //[cite: 42]
      />

      <main className={styles.mainContent}>
        {/* O Outlet renderiza o componente da rota filha definida no router.tsx */}
        <Outlet />
      </main>
    </div>
  );
};