import React from 'react';
import type { NavigationItem } from './types';
import './Sidebar.scss';

interface SidebarProps {
  id: string;
  items: NavigationItem[];
  activeId: string;
  onNavigate: (id: string) => void;
  isOpenMobile: boolean; // Controla se o menu está aberto no telemóvel
  onCloseMobile: () => void;
  onToggleMobile?: () => void;
  showMobileSubheader?: boolean;
}

export const Sidebar: React.FC<SidebarProps> = ({ 
  id,
  items, 
  activeId, 
  onNavigate,
  isOpenMobile,
  onCloseMobile,
  onToggleMobile,
  showMobileSubheader = false,
}) => {
  const activeItemLabel = items.find((item) => item.id === activeId)?.label || 'Menu';

  return (
    <>
      {showMobileSubheader && (
        /* Sub-header Mobile Gerado Automaticamente pela Sidebar */
        <header className="sidebar__mobile-subheader">
          <button
            type="button"
            className="sidebar__mobile-toggle"
            onClick={(e) => {
              e.stopPropagation();
              onToggleMobile?.();
            }}
            aria-label={isOpenMobile ? 'Fechar menu lateral' : 'Abrir menu lateral'}
            aria-expanded={isOpenMobile}
          >
            <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h16" /></svg>
          </button>
          <span className="sidebar__mobile-title">{activeItemLabel}</span>
        </header>
      )}

      {/* Overlay escuro para mobile: clica fora, fecha o menu */}
      {isOpenMobile && (
        <div
          className="sidebar-overlay"
          onClick={(e) => {
            e.stopPropagation();
            onCloseMobile();
          }}
          aria-hidden="true"
        />
      )}

      <aside id={id} className={`sidebar ${isOpenMobile ? 'sidebar--open' : ''}`}>
        <div className="sidebar__header">
          <h1 className="sidebar__logo">Bueiro Inteligente</h1>
          {/* Opcional: botão de fechar para mobile */}
          <button
            className="sidebar__close-btn"
            onClick={(e) => {
              e.stopPropagation();
              onCloseMobile();
            }}
          >
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
                    onClick={(e) => {
                      e.stopPropagation();
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