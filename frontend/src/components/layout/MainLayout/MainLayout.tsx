import React from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Navbar } from '../Navbar/Navbar';
import { Footer } from '../Footer/Footer';
import { Sidebar } from '../Sidebar/Sidebar';
import { BottomBar } from '../BottomBar/BottomBar';
import { Info, Home } from 'lucide-react'; 
import type { NavigationItem } from '../Sidebar/types';
import styles from './MainLayout.module.scss';

interface MainNavigationItem extends NavigationItem {
  path?: string;
}

export const MainLayout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  // Removido o item 'dash' (Monitoramento) para que não apareça na Sidebar ou BottomBar
  const navItems: MainNavigationItem[] = [
    { 
      id: 'home', 
      label: 'Home', 
      path: '/', 
      icon: <Home size={20} />, 
      component: <></> 
    },
    { 
      id: 'about', 
      label: 'Sobre nós', 
      path: '/sobre', 
      icon: <Info size={20} />, 
      component: <></> 
    },
  ];

  // Encontra o item ativo com base na rota atual para destacar o ícone correto
  const activeItem = navItems.find(item => item.path === location.pathname) || navItems[0];
  
  // Filtra itens que possuem caminho para a barra de navegação inferior (mobile)
  const bottomBarItems = navItems.filter((item): item is MainNavigationItem & { path: string } => Boolean(item.path));

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
          <Outlet /> {/* Aqui a Home agora exibirá os dados de monitoramento conforme sua nova regra */}
        </main>
      </div>

      <BottomBar 
        items={bottomBarItems} 
        activeId={activeItem.id} 
        onNavigate={handleNavigate} 
      />

      <Footer />
    </div>
  );
};