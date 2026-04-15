import React, { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../../../feature/auth/hooks/useAuth';

import styles from './Navbar.module.scss';

export const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);


  return (
    <header className={styles.header}>
      <div className={styles.container}>
        {/* LOGO */}
        <NavLink to="/" className={styles.logo} aria-label="Página Inicial do Bueiro Inteligente">
          Bueiro Inteligente
        </NavLink>

        {/* NAV MENU E AUTENTICAÇÃO */}
        <nav className={styles.navMenu} aria-label="Menu da conta">
          <div className={styles.navLinks}>
            <NavLink
              to="/sobre"
              className={({ isActive }) => `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`}
            >
              Sobre nós
            </NavLink>
          </div>

          <div className={styles.authSection}>
            {user ? (
              <div className={styles.userDropdownContainer}>
                <button 
                  className={styles.userInfoBtn} 
                  onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                  type="button"
                  aria-haspopup="menu"
                  aria-expanded={isDropdownOpen}
                >
                  Olá, {user.full_name} ▾
                </button>
                {isDropdownOpen && (
                  <div className={styles.dropdownMenu} role="menu" aria-label="Menu do utilizador">
                    <button
                      className={styles.logoutBtn}
                      onClick={async () => {
                        setIsDropdownOpen(false);
                        await logout();
                      }}
                      type="button"
                      role="menuitem"
                    >
                      Sair da Conta
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <NavLink to="/login" className={styles.loginButton}>Entrar</NavLink>
            )}
          </div>
        </nav>
      </div>
    </header>
  );
};
