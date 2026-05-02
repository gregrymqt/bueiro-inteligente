import React from 'react';
import './StatusBadge.scss';

interface StatusBadgeProps {
  status: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  // Converte o status do back-end para uma classe CSS amigável
  const statusClass = status.toLowerCase();

  return (
    <span className={`status-badge status-badge--${statusClass}`}>
      {status}
    </span>
  );
};