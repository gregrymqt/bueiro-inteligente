import React, { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { Menu, X, User } from 'lucide-react';
import { useAuth } from '../../../feature/auth/hooks/useAuth';

import styles from './Navbar.module.scss';

interface NavbarProps {
  onOpenMenu: () => void;
}
export const Navbar: React.FC<NavbarProps> = ({ onOpenMenu }) => {
  const { user } = useAuth();


  return (
    <header className={styles.header}>
      <div className={styles.container}>
        {/* LOGO */}
        <NavLink to="/" className={styles.logo} aria-label="Página Inicial do Bueiro Inteligente">
          Bueiro Inteligente
        </NavLink>

        {/* BOTÃO MOBILE (HAMBÚRGUER) */}
        <button className={styles.hamburger} onClick={onOpenMenu} aria-label="Abrir Menu">
          <Menu size={24} />
        </button>

        {/* NAV MENU E AUTENTICAÇÃO */}
        <nav className={styles.desktopNav}>
          <div className={styles.links}>
            <NavLink to="/" className={({ isActive }) => `${styles.navLink} ${isActive ? styles.active : ''}`}>
              Home
            </NavLink>
            <NavLink to="/dashboard" className={({ isActive }) => `${styles.navLink} ${isActive ? styles.active : ''}`}>
              Monitoramento
            </NavLink>
          </div>

          <div className={styles.authSection}>
            {user ? (
              <span className={styles.userInfo}>Olá, {user.username}</span>
            ) : (
              <NavLink to="/login" className={styles.loginButton}>Entrar</NavLink>
            )}
          </div>
        </nav>
      </div>
    </header>
  );
};
