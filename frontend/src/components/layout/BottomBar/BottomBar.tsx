import React from 'react';
import type { NavigationItem } from '../Sidebar/types';
import styles from './BottomBar.module.scss';

interface BottomBarItem extends NavigationItem {
  path: string;
}

interface BottomBarProps {
  items: BottomBarItem[];
  activeId: string;
  onNavigate: (id: string) => void;
}

export const BottomBar: React.FC<BottomBarProps> = ({ items, activeId, onNavigate }) => {
  return (
    <nav className={styles.bottomBar} aria-label="Navegação principal mobile">
      <ul className={styles.list}>
        {items.map((item) => {
          const isActive = item.id === activeId;

          return (
            <li key={item.id} className={styles.item}>
              <button
                type="button"
                className={`${styles.button} ${isActive ? styles.buttonActive : ''}`}
                onClick={() => onNavigate(item.id)}
                aria-current={isActive ? 'page' : undefined}
              >
                <span className={styles.icon}>{item.icon}</span>
                <span className={styles.label}>{item.label}</span>
              </button>
            </li>
          );
        })}
      </ul>
    </nav>
  );
};