import React, { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { Menu, X, User } from 'lucide-react';
// Aqui assumimos o caminho da sua feature de auth:
 import { useAuth } from '../../../feature/auth/hooks/useAuth';

import styles from './Navbar.module.scss';

export const Navbar: React.FC = () => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  // Integração com o hook de autenticação (Descomente o import e use a linha abaixo no projeto real):
  const { user } = useAuth();
  
  // Mock para fins de demonstração (Remova após plugar o hook verdadeiro):
  // const user = null; // troque para { name: 'João' } para testar logado

  const toggleMenu = () => setIsMenuOpen((prev) => !prev);

  return (
    <header className={styles.header}>
      <div className={styles.container}>
        {/* LOGO */}
        <NavLink to="/" className={styles.logo} aria-label="Página Inicial do Bueiro Inteligente">
          Bueiro Inteligente
        </NavLink>

        {/* BOTÃO MOBILE (HAMBÚRGUER) */}
        <button 
          className={styles.hamburger} 
          onClick={toggleMenu}
          aria-label={isMenuOpen ? 'Fechar menu de navegação' : 'Abrir menu de navegação'}
          aria-expanded={isMenuOpen}
        >
          {isMenuOpen ? <X size={24} /> : <Menu size={24} />}
        </button>

        {/* NAV MENU E AUTENTICAÇÃO */}
        <nav className={`${styles.navMenu} ${isMenuOpen ? styles.isOpen : ''}`}>
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
              <div className={styles.userInfo}>
                <User size={20} />
                <span>Olá, {user.name}</span>
              </div>
            ) : (
              <NavLink to="/auth" className={styles.loginButton} aria-label="Entrar no sistema">
                Entrar
              </NavLink>
            )}
          </div>
        </nav>
      </div>
    </header>
  );
};
