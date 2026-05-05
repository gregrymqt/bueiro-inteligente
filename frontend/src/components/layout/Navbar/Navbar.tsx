import React, { useEffect, useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../feature/auth/hooks/useAuth';
import { NotificationService } from '../../../feature/notifications/services/NotificationService';
import styles from './Navbar.module.scss';

export const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0); // Estado para o Badge
  const canAccessAdminPanel = user?.roles.includes('admin') ?? false;

  useEffect(() => {
    if (!user) return;

    const fetchInitialCount = async () => {
      try {
        const count = await NotificationService.getUnreadCount();
        setUnreadCount(count);
      } catch (error) {
        console.error("Erro ao buscar notificações", error);
      }
    };

    fetchInitialCount();

    // 2. OUVINTE: Escuta o "grito" do sistema de Realtime
    const handleBadgeUpdate = () => {
      // Incrementa o contador local ou faz um novo fetch para precisão total
      setUnreadCount(prev => prev + 1);
    };

    window.addEventListener('badge:update', handleBadgeUpdate);

    return () => {
      window.removeEventListener('badge:update', handleBadgeUpdate);
    };
  }, [user]);

  return (
    <header className={styles.header}>
      <div className={styles.container}>
        {/* LOGO */}
        <NavLink to="/" className={styles.logo} aria-label="Página Inicial do Bueiro Inteligente">
          Bueiro Inteligente
        </NavLink>

        {/* NAV MENU E AUTENTICAÇÃO */}
        <nav className={styles.navMenu} aria-label="Menu da conta">
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
                  <div className={styles.notificationWrapper}>
                    <span className={styles.bellIcon}>🔔</span>
                    {unreadCount > 0 && (
                      <span className={styles.badge}>{unreadCount}</span>
                    )}
                  </div>
                  Olá, {user.full_name} ▾
                </button>
                {isDropdownOpen && (
                  <div className={styles.dropdownMenu} role="menu" aria-label="Menu do utilizador">
                    {canAccessAdminPanel && (
                      <button
                        type="button"
                        className={styles.logoutBtn}
                        onClick={() => {
                          navigate('/admin');
                          setIsDropdownOpen(false);
                        }}
                        role="menuitem"
                      >
                        Painel Admin
                      </button>
                    )}
                    <button
                      type="button"
                      className={styles.logoutBtn}
                      onClick={async () => {
                        setIsDropdownOpen(false);
                        await logout();
                      }}
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
