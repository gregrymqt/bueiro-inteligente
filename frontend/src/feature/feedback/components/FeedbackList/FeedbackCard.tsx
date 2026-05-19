import React from 'react';
import type { Feedback } from '../../types';
import styles from './FeedbackList.module.scss';
import { PencilLine, Star, Trash2 } from 'lucide-react';
import { Card } from '@/components/ui/Card/Card';

interface FeedbackCardProps {
  feedback: Feedback;
  onEdit?: (feedback: Feedback) => void;
  onDelete?: (id: string) => void;
  isActionLoading?: boolean;
}

export const FeedbackCard: React.FC<FeedbackCardProps> = ({ 
  feedback, onEdit, onDelete, isActionLoading 
}) => {
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
          alt={feedback.userName || 'Usuário'} 
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

  // 🔥 Botões transformados em ações iconográficas polidas e discretas
  const actionsContent = (onEdit || onDelete) && (
    <div className={styles.cardActions}>
      {onEdit && (
        <button
          type="button"
          className={styles.editBtn}
          onClick={() => onEdit(feedback)}
          disabled={isActionLoading}
          title="Editar Avaliação"
        >
          <PencilLine size={15} />
        </button>
      )}
      {onDelete && (
        <button
          type="button"
          className={styles.deleteBtn}
          onClick={() => onDelete(feedback.id)}
          disabled={isActionLoading}
          title="Excluir Avaliação"
        >
          <Trash2 size={15} />
        </button>
      )}
    </div>
  );

  return (
    <Card className={styles.card} footer={authorContent}>
      {/* 🔥 Divisor espacial estratégico criado aqui */}
      <div className={styles.cardHeader}>
        <div className={styles.rating}>
          {renderStars(feedback.rating)}
        </div>
        {actionsContent}
      </div>
      
      {/* Removemos as aspas estáticas do texto, deixando o CSS tratar a semântica */}
      <p className={styles.comment}>{feedback.comment}</p>
    </Card>
  );
};