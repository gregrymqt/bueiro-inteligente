import React from 'react';
import { NavLink } from 'react-router-dom';
import { Menu } from 'lucide-react';
import { useAuth } from '../../../feature/auth/hooks/useAuth';

import styles from './Navbar.module.scss';

interface NavbarProps {
  onOpenMenu: () => void;
  isMobileMenuOpen: boolean;
}
export const Navbar: React.FC<NavbarProps> = ({ onOpenMenu, isMobileMenuOpen }) => {
  const { user } = useAuth();


  return (
    <header className={styles.header}>
      <div className={styles.container}>
        {/* LOGO */}
        <NavLink to="/" className={styles.logo} aria-label="Página Inicial do Bueiro Inteligente">
          Bueiro Inteligente
        </NavLink>

        {/* BOTÃO MOBILE (HAMBÚRGUER) */}
        <button className={styles.hamburger} type="button" onClick={onOpenMenu} aria-label="Abrir Menu" aria-expanded={isMobileMenuOpen}>
          <Menu size={24} />
        </button>

        {/* NAV MENU E AUTENTICAÇÃO */}
        <nav className={`${styles.navMenu} ${isMobileMenuOpen ? styles.isOpen : ''}`} aria-label="Menu da conta">
          <div className={styles.authSection}>
            {user ? (
              <span className={styles.userInfo}>Olá, {user.full_name}</span>
            ) : (
              <NavLink to="/login" className={styles.loginButton}>Entrar</NavLink>
            )}
          </div>
        </nav>
      </div>
    </header>
  );
};
