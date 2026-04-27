import React, { useState } from 'react';
import type { NavigationItem } from './types';
import './Sidebar.scss';

const findNavigationItem = (
  items: NavigationItem[],
  targetId: string,
): NavigationItem | undefined => {
  for (const item of items) {
    if (item.id === targetId) {
      return item;
    }

    const nestedItem = item.children ? findNavigationItem(item.children, targetId) : undefined;

    if (nestedItem) {
      return nestedItem;
    }
  }

  return undefined;
};

const containsItemId = (items: NavigationItem[] | undefined, targetId: string): boolean => {
  if (!items) {
    return false;
  }

  return items.some((item) => item.id === targetId || containsItemId(item.children, targetId));
};

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
  const [expandedItems, setExpandedItems] = useState<Set<string>>(() => new Set());

  const activeItemLabel = findNavigationItem(items, activeId)?.label || 'Menu';

  const toggleExpandedItem = (itemId: string) => {
    setExpandedItems((currentExpandedItems) => {
      const nextExpandedItems = new Set(currentExpandedItems);

      if (nextExpandedItems.has(itemId)) {
        nextExpandedItems.delete(itemId);
      } else {
        nextExpandedItems.add(itemId);
      }

      return nextExpandedItems;
    });
  };

  const renderNavigationItems = (
    navigationItems: NavigationItem[],
    isChildLevel = false,
    isVisible = true,
  ) => (
    <>
      {navigationItems.map((item) => {
        const hasChildren = Boolean(item.children?.length);
        const hasActiveDescendant = containsItemId(item.children, activeId);
        const isCurrent = item.id === activeId;
        const isActive = isCurrent || hasActiveDescendant;
        const isExpanded = expandedItems.has(item.id) || hasActiveDescendant;
        const isNestedVisible = isVisible && isExpanded;

        return (
          <li key={item.id} className="sidebar__item">
            <button
              type="button"
              className={[
                'sidebar__btn',
                isActive ? 'sidebar__btn--active' : '',
                hasChildren ? 'sidebar__btn--parent' : '',
                isChildLevel ? 'sidebar__btn--child' : '',
              ]
                .filter(Boolean)
                .join(' ')}
              tabIndex={isVisible ? 0 : -1}
              onClick={(e) => {
                e.stopPropagation();

                if (hasChildren) {
                  toggleExpandedItem(item.id);
                  return;
                }

                onNavigate(item.id);
                onCloseMobile();
              }}
              aria-current={isCurrent ? 'page' : undefined}
              aria-expanded={hasChildren ? isExpanded : undefined}
              aria-haspopup={hasChildren ? 'true' : undefined}
            >
              <span className="sidebar__btn-content">
                <span className="sidebar__icon">{item.icon}</span>
                <span className="sidebar__label">{item.label}</span>
              </span>

              {hasChildren && (
                <span
                  className={`sidebar__chevron ${isExpanded ? 'sidebar__chevron--expanded' : ''}`}
                  aria-hidden="true"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </span>
              )}
            </button>

            {hasChildren && (
              <ul
                className={[
                  'sidebar__list',
                  'sidebar__list--nested',
                  isExpanded ? 'sidebar__list--nested--expanded' : '',
                ]
                  .filter(Boolean)
                  .join(' ')}
                aria-hidden={!isNestedVisible}
              >
                {renderNavigationItems(item.children ?? [], true, isNestedVisible)}
              </ul>
            )}
          </li>
        );
      })}
    </>
  );

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
          <h1 className="sidebar__logo" onClick={(e) => {
            e.stopPropagation();
            onNavigate('home');
            onCloseMobile();
          }}>
            Bueiro Inteligente
          </h1>
          {/* Opcional: botão de fechar para mobile */}
          <button
            type="button"
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
          {renderNavigationItems(items)}
        </ul>
      </nav>
    </aside >
    </>
  );
};