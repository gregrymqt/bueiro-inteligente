import React from 'react';
import type { Feedback } from '../../types';
import styles from './FeedbackList.module.scss';
import { PencilLine, Star, Trash2 } from 'lucide-react';
import { Card } from '@/components/ui/Card/Card';
import { Button } from '@/components/ui/Button/Button';

interface FeedbackCardProps {
  feedback: Feedback;
  onEdit?: (feedback: Feedback) => void;
  onDelete?: (id: string) => void;
  isActionLoading?: boolean;
}

export const FeedbackCard: React.FC<FeedbackCardProps> = ({ 
  feedback, onEdit, onDelete, isActionLoading 
}) => {
  // 1. Fallback seguro para as iniciais (Evita TypeError)
  const initial = feedback.userName?.charAt(0)?.toUpperCase() || '?';

  const renderStars = (rating: number) => {
    return Array.from({ length: 5 }).map((_, index) => (
      <Star
        key={index}
        size={16}
        className={index < rating ? styles.starFilled : styles.starEmpty}
      />
    ));
  };

  const authorContent = (
    <div className={styles.author}>
      {feedback.avatarUrl ? (
        <img 
          src={feedback.avatarUrl} 
          alt={feedback.userName || 'Usuário'} // Fallback no alt text
          className={styles.avatar} 
        />
      ) : (
        <div className={styles.avatarPlaceholder}>
          {initial}
        </div>
      )}
      <div className={styles.info}>
        <strong className={styles.name}>{feedback.userName || 'Anônimo'}</strong>
        <span className={styles.role}>{feedback.role}</span>
      </div>
    </div>
  );  

  const actionsContent = (onEdit || onDelete) && (
    <div className={styles.cardActions}>
      {onEdit && (
        <Button
          variant="secondary" size="sm"
          leftIcon={<PencilLine size={14} />}
          onClick={() => onEdit(feedback)}
          disabled={isActionLoading}
        >Editar</Button>
      )}
      {onDelete && (
        <Button
          variant="danger" size="sm"
          leftIcon={<Trash2 size={14} />}
          onClick={() => onDelete(feedback.id)}
          disabled={isActionLoading}
        >Excluir</Button>
      )}
    </div>
  );

  return (
    <Card className={styles.card} footer={authorContent}>
      <div className={styles.rating}>
        {renderStars(feedback.rating)}
        {actionsContent} {/* Renderiza botões se estiver na Dashboard[cite: 41] */}
      </div>
      <p className={styles.comment}>"{feedback.comment}"</p>
    </Card>
  );
};