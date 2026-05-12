import React from 'react';
import { Outlet, NavLink } from 'react-router-dom';
import styles from './CheckoutLayout.module.scss';
import { useAuth } from '@/feature/auth/hooks/useAuth';

export const CheckoutLayout: React.FC = () => {
  const { user } = useAuth();
  
  return (
    <div className={styles.layoutWrapper}>
      <header className={styles.simpleHeader}>
        <div className={styles.container}>
          <NavLink to="/" className={styles.logo} aria-label="Página Inicial do Bueiro Inteligente">
            Bueiro Inteligente
          </NavLink>
          {user && (
            <div className={styles.userInfo}>
              <span>Olá, {user.full_name}</span>
            </div>
          )}
        </div>
      </header>
      <main className={styles.mainContent}>
        <Outlet />
      </main>
    </div>
  );
};
