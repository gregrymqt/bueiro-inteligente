import React, { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Navbar } from '../Navbar/Navbar';
import { Footer } from '../Footer/Footer';
import { Sidebar } from '../Sidebar/Sidebar';
import { LayoutDashboard, Info, Home } from 'lucide-react'; // Ícones para a sidebar
import styles from './MainLayout.module.scss';

export const MainLayout: React.FC = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  // Itens de navegação compartilhados entre os componentes
  const navItems = [
    { id: 'home', label: 'Home', path: '/', icon: <Home size={20} />, component: <></> },
    { id: 'dash', label: 'Monitoramento', path: '/dashboard', icon: <LayoutDashboard size={20} />, component: <></> },
    { id: 'about', label: 'Sobre o Projeto', path: '/sobre', icon: <Info size={20} />, component: <></> },
  ];

  const toggleSidebar = () => setIsSidebarOpen(!isSidebarOpen);

  const activeItem = navItems.find(item => item.path === location.pathname) || navItems[0];

  const handleNavigate = (id: string) => {
    const item = navItems.find(i => i.id === id);
    if (item && item.path) {
      navigate(item.path);
    }
  };

  return (
    <div className={styles.layoutWrapper}>
      <Navbar onOpenMenu={toggleSidebar} />
      
      <div className={styles.container}>
        <Sidebar 
          items={navItems} 
          activeId={activeItem.id}
          onNavigate={handleNavigate}
          isOpenMobile={isSidebarOpen} 
          onCloseMobile={() => setIsSidebarOpen(false)} 
        />
        
        <main className={styles.content}>
          <Outlet /> {/* Aqui entram as páginas: Home, Dashboard, etc. */}
        </main>
      </div>

      <Footer />
    </div>
  );
};