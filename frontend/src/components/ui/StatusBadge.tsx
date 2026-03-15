import React from 'react';

// 1. A tipagem estrita que espelha o payload do seu FastAPI
export type DrainStatus = 'normal' | 'alerta' | 'critico';

interface StatusBadgeProps {
  status: DrainStatus;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  // 2. Um dicionário de configurações elimina a necessidade de if/else gigantes
  const statusConfig: Record<DrainStatus, { label: string; modifierClass: string }> = {
    normal: { label: 'Normal', modifierClass: 'badge--success' },
    alerta: { label: 'Atenção', modifierClass: 'badge--warning' },
    critico: { label: 'Crítico', modifierClass: 'badge--danger' },
  };

  const currentConfig = statusConfig[status];

  // 3. Renderização limpa aplicando as classes do seu SCSS
  return (
    <span className={`badge ${currentConfig.modifierClass}`}>
      <span className="badge__dot" aria-hidden="true" />
      {currentConfig.label}
    </span>
  );
};