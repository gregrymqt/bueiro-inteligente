import React, { type ReactNode } from 'react';
import { Card } from '../../../components/ui/Card/Card';
import './StatCard.scss';

export interface StatCardProps {
  icon: ReactNode;
  value: string | number;
  description: string;
  color?: 'primary' | 'success' | 'warning' | 'danger';
}

export const StatCard: React.FC<StatCardProps> = ({ 
  icon, 
  value, 
  description, 
  color = 'primary' 
}) => {
  return (
    <Card className={`stat-card stat-card--${color}`}>
      <div className="stat-card__content">
        <div className={`stat-card__icon stat-card__icon--${color}`}>
          {icon}
        </div>
        <div className="stat-card__info">
          <span className="stat-card__value">{value}</span>
          <span className="stat-card__description">{description}</span>
        </div>
      </div>
    </Card>
  );
};
