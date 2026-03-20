import React from 'react';
import { NavLink } from 'react-router-dom';
import styles from './Footer.module.scss';

export const Footer: React.FC = () => {
  return (
    <footer className={styles.footer} aria-label="Rodapé do site">
      <div className={styles.container}>
        
        {/* Informações da Marca/Copyright */}
        <div className={styles.brandInfo}>
          <span className={styles.copy}>
            &copy; {new Date().getFullYear()} Bueiro Inteligente.
          </span>
          <span className={styles.rights}>Todos os direitos reservados.</span>
        </div>

        {/* Links Rápidos */}
        <nav className={styles.links} aria-label="Links rápidos do rodapé">
          <NavLink to="/" className={styles.link}>Home</NavLink>
          <NavLink to="/dashboard" className={styles.link}>Monitoramento</NavLink>
          <NavLink to="/sobre" className={styles.link}>Sobre o Projeto</NavLink>
        </nav>

        {/* Indicador de Versão */}
        <div className={styles.versionContainer}>
          <span className={styles.versionBadge} aria-label="Versão do sistema">v1.0.0</span>
        </div>

      </div>
    </footer>
  );
};
