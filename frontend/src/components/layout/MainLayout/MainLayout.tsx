import React from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Navbar } from '../Navbar/Navbar';
import { Footer } from '../Footer/Footer';
import { Sidebar } from '../Sidebar/Sidebar';
import { BottomBar } from '../BottomBar/BottomBar';
import { LayoutDashboard, Info, Home } from 'lucide-react'; // Ícones para a sidebar
import type { NavigationItem } from '../Sidebar/types';
import styles from './MainLayout.module.scss';

interface MainNavigationItem extends NavigationItem {
  path: string;
}

export const MainLayout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  // Itens de navegação compartilhados entre os componentes
  const navItems: MainNavigationItem[] = [
    { id: 'home', label: 'Home', path: '/', icon: <Home size={20} />, component: <></> },
    { id: 'dash', label: 'Monitoramento', path: '/dashboard', icon: <LayoutDashboard size={20} />, component: <></> },
    { id: 'about', label: 'Sobre nós', path: '/sobre', icon: <Info size={20} />, component: <></> },
  ];

  const activeItem = navItems.find(item => item.path === location.pathname) || navItems[0];

  const handleNavigate = (id: string) => {
    const item = navItems.find(i => i.id === id);
    if (item && item.path) {
      navigate(item.path);
    }
  };

  return (
    <div className={styles.layoutWrapper}>
      <Navbar />
      
      <div className={styles.container}>
        <div className={styles.sidebarDesktop}>
          <Sidebar 
            id="global-sidebar"
            items={navItems} 
            activeId={activeItem.id}
            onNavigate={handleNavigate}
            isOpenMobile={false}
            onCloseMobile={() => {}}
          />
        </div>
        
        <main className={styles.content}>
          <Outlet /> {/* Aqui entram as páginas: Home, Dashboard, etc. */}
        </main>
      </div>

      <BottomBar items={navItems} activeId={activeItem.id} onNavigate={handleNavigate} />

      <Footer />
    </div>
  );
};