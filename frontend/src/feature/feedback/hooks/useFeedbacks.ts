import { useState, useCallback } from 'react';
import { FeedbackService } from '../services/FeedbackService';
import { AlertService } from '@/core/alert/AlertService';
import type { Feedback } from '../types';

export function useFeedbacks() {
  const [feedbacks, setFeedbacks] = useState<Feedback[]>([]);
  const [loading, setLoading] = useState(true);
  const [isDeleting, setIsDeleting] = useState(false);

  const fetchFeedbacks = useCallback(async () => {
    setLoading(true);
    try {
      const data = await FeedbackService.getFeedbacks();
      setFeedbacks(data);
    } catch {
      AlertService.error('Erro', 'Não foi possível carregar as avaliações.');
    } finally {
      setLoading(false);
    }
  }, []);

 const removeFeedback = async (id: string) => {
    await AlertService.confirm({
      title: 'Tem certeza?',
      text: 'Esta ação não pode ser desfeita.',
      onConfirm: async () => {
        setIsDeleting(true);
        try {
          await FeedbackService.deleteFeedback(id);
          setFeedbacks((prev) => prev.filter((f) => f.id !== id));
          AlertService.success('Removido', 'Sua avaliação foi excluída.');
        } catch (error) {
          AlertService.error('Erro', error instanceof Error ? error.message : 'Falha ao remover a avaliação.');
        } finally {
          setIsDeleting(false);
        }
      },
    });
  };

  return { feedbacks, loading, isDeleting, removeFeedback, refresh: fetchFeedbacks };
}