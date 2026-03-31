import React from 'react';
import { Card } from '../../../components/ui/Card/Card';
import * as Icons from 'lucide-react';
import './StatCard.scss';

export interface StatCardProps {
  iconName: string;
  title: string;
  value: string | number;
  description: string;
  color?: 'primary' | 'success' | 'warning' | 'danger';
}

export const StatCard: React.FC<StatCardProps> = ({ 
  iconName,
  title,
  value, 
  description, 
  color = 'primary' 
}) => {
  // Tenta encontrar o ícone no mapa de componentes da Lucide. Se não achar, usa um ícone de fallback (HelpCircle)
  const IconComponent = (Icons as unknown as Record<string, React.ElementType>)[iconName] || Icons.HelpCircle;

  return (
    <Card className={`stat-card stat-card--${color}`}>
      <div className="stat-card__content">
        <div className="stat-card__header">
          <span className="stat-card__title">{title}</span>
          <div className={`stat-card__icon stat-card__icon--${color}`}>
            <IconComponent size={24} />
          </div>
        </div>
        <div className="stat-card__info">
          <span className="stat-card__value">{value}</span>
          <span className="stat-card__description">{description}</span>
        </div>
      </div>
    </Card>
  );
};
