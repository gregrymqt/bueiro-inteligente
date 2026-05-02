import React from 'react';
import { useFeedbacks } from '../../hooks/useFeedbacks';
import { ReviewsSkeleton } from '@/feature/home/components/HomeSkeletons/HomeSkeletons'; // Reutilizando seu skeleton[cite: 15]
import styles from './FeedbackList.module.scss';
import { FeedbackCard } from './FeedbackCard';
import type { Feedback } from '../../types';

export const FeedbackList: React.FC<{
  onEditFeedback?: (f: Feedback) => void;
}> = ({ onEditFeedback }) => {
  const { feedbacks, loading, isDeleting, removeFeedback } = useFeedbacks();

  if (loading) {
    return <ReviewsSkeleton />; // Mantém a consistência visual durante o fetch[cite: 15]
  }

  if (feedbacks.length === 0) {
    return (
      <div className={styles.emptyState}>
        <p>Ainda não há avaliações para exibir.</p>
      </div>
    );
  }

  return (
    <div className={styles.listContainer}>
      <div className={styles.feedbackGrid}>
        {feedbacks.map((feedback) => (
          <FeedbackCard
            key={feedback.id}
            feedback={feedback}
            onEdit={onEditFeedback}
            onDelete={removeFeedback}
            isActionLoading={isDeleting}
          />
        ))}
      </div>
    </div>
  );
};