import React from 'react';
import { isMockDataSourceEnabled, resolveDataSourceMode } from '@/core/http/environment';
import styles from './DemoModeBadge.module.scss';

export const DemoModeBadge: React.FC = () => {
  const isDemoMode = isMockDataSourceEnabled();
  const modeLabel = isDemoMode ? 'Modo demonstração' : 'Backend ativo';
  const modeDescription = isDemoMode ? 'Dados locais / mock' : 'API e realtime reais';

  return (
    <div
      className={`${styles.badge} ${isDemoMode ? styles.mock : styles.backend}`}
      aria-label={`Estado atual da aplicação: ${resolveDataSourceMode()}`}
      title={isDemoMode ? 'Aplicação em modo demonstração' : 'Aplicação conectada ao backend'}
    >
      <span className={styles.dot} aria-hidden="true" />

      <div className={styles.textBlock}>
        <strong className={styles.label}>{modeLabel}</strong>
        <span className={styles.description}>{modeDescription}</span>
      </div>
    </div>
  );
};