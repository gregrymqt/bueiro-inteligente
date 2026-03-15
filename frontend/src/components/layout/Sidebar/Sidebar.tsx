import React from 'react';
import type { NavigationItem } from './types';
import './Sidebar.scss';

interface SidebarProps {
  items: NavigationItem[];
  activeId: string;
  onNavigate: (id: string) => void;
  isOpenMobile: boolean; // Controla se o menu está aberto no telemóvel
  onCloseMobile: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ 
  items, 
  activeId, 
  onNavigate,
  isOpenMobile,
  onCloseMobile
}) => {
  return (
    <>
      {/* Overlay escuro para mobile: clica fora, fecha o menu */}
      {isOpenMobile && (
        <div className="sidebar-overlay" onClick={onCloseMobile} aria-hidden="true" />
      )}

      <aside className={`sidebar ${isOpenMobile ? 'sidebar--open' : ''}`}>
        <div className="sidebar__header">
          <h1 className="sidebar__logo">Bueiro Inteligente</h1>
          {/* Opcional: botão de fechar para mobile */}
          <button className="sidebar__close-btn" onClick={onCloseMobile}>
            &times;
          </button>
        </div>

        <nav className="sidebar__nav">
          <ul className="sidebar__list">
            {items.map((item) => {
              const isActive = item.id === activeId;

              return (
                <li key={item.id} className="sidebar__item">
                  <button
                    className={`sidebar__btn ${isActive ? 'sidebar__btn--active' : ''}`}
                    onClick={() => {
                      onNavigate(item.id);
                      onCloseMobile(); // Fecha o menu no mobile após clicar
                    }}
                    aria-current={isActive ? 'page' : undefined}
                  >
                    <span className="sidebar__icon">{item.icon}</span>
                    <span className="sidebar__label">{item.label}</span>
                  </button>
                </li>
              );
            })}
          </ul>
        </nav>
      </aside>
    </>
  );
};