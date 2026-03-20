import React, { type ReactNode } from 'react';
import './Card.scss';

export interface CardProps {
  /**
   * Título principal do card. Pode ser texto ou outro componente.
   */
  title?: ReactNode;
  /**
   * Subtítulo do card. Fica logo abaixo do título.
   */
  subtitle?: ReactNode;
  /**
   * Conteúdo principal do card.
   */
  children: ReactNode;
  /**
   * Conteúdo de rodapé (geralmente usado para botões de ação ou informações extras).
   */
  footer?: ReactNode;
  /**
   * Classes extras para estilização customizada no wrapper principal.
   */
  className?: string;
  /**
   * Evento de clique no card inteiro. Se passado, adicionará feedback visual de hover.
   */
  onClick?: () => void;
}

export const Card: React.FC<CardProps> = ({
  title,
  subtitle,
  children,
  footer,
  className = '',
  onClick,
}) => {
  return (
    <div 
      className={`generic-card ${onClick ? 'clickable' : ''} ${className}`} 
      onClick={onClick}
    >
      {(title || subtitle) && (
        <div className="generic-card__header">
          {title && <h3 className="generic-card__title">{title}</h3>}
          {subtitle && <p className="generic-card__subtitle">{subtitle}</p>}
        </div>
      )}
      
      <div className="generic-card__body">
        {children}
      </div>

      {footer && (
        <div className="generic-card__footer">
          {footer}
        </div>
      )}
    </div>
  );
};
