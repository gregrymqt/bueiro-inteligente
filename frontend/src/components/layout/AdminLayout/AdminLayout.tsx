import { useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Mail, LayoutDashboard, Menu } from 'lucide-react';
import { Button } from '@/components/ui/Button/Button';
import { Sidebar } from '../Sidebar/Sidebar';
import type { NavigationItem } from '../Sidebar/types';
import styles from './AdminLayout.module.scss';

interface AdminNavigationItem extends NavigationItem {
  path: string;
}

export const AdminLayout = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  const navItems: AdminNavigationItem[] = [
    {
      id: 'drains',
      label: 'Bueiros',
      path: '/admin/drains',
      icon: <Droplets size={20} />,
      component: <></>,
    },
    {
      id: 'home',
      label: 'Gestão da Home',
      path: '/admin/home',
      icon: <LayoutDashboard size={20} />,
      component: <></>,
    },
    {
      id: 'messages',
      label: 'Mensagens',
      path: '/admin/messages',
      icon: <Droplets size={20} />,
      component: <></>,
    },
  ];

  const activeItem =
    navItems.find(
      (item) => location.pathname === item.path || location.pathname.startsWith(`${item.path}/`)
    ) ?? navItems[0];

  const handleNavigate = (id: string) => {
    const targetItem = navItems.find((item) => item.id === id);

    if (targetItem) {
      navigate(targetItem.path);
      setIsSidebarOpen(false);
    }
  };

  return (
    <div className={styles.layout}>
      <Sidebar
        id="admin-sidebar"
        items={navItems}
        activeId={activeItem.id}
        onNavigate={handleNavigate}
        isOpenMobile={isSidebarOpen}
        onCloseMobile={() => setIsSidebarOpen(false)}
      />

      <div className={styles.mainColumn}>
        <header className={styles.toolbar}>
          <Button
            type="button"
            variant="secondary"
            size="sm"
            leftIcon={<Menu size={16} />}
            className={styles.menuButton}
            onClick={() => setIsSidebarOpen(true)}
          >
            Menu
          </Button>

          <span className={styles.toolbarLabel}>Painel Administrativo</span>
        </header>

        <main className={styles.content}>
          <div className={styles.contentInner}>
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
};