import { useState } from 'react';
import { FeedbackService } from '../services/FeedbackService';
import { AlertService } from '@/core/alert/AlertService';
import type { FeedbackCreatePayload, FeedbackUpdatePayload } from '../types';

export function useFeedbackForm() {
  const [isSubmitting, setIsSubmitting] = useState(false);

  const sendFeedback = async (payload: FeedbackCreatePayload) => {
    setIsSubmitting(true);
    try {
      await FeedbackService.submitFeedback(payload);
      AlertService.success('Obrigado!', 'Seu feedback foi enviado para moderação.');
      return true;
    } catch (error) {
      AlertService.error('Erro', error instanceof Error ? error.message : 'Não foi possível enviar seu feedback.');
      return false;
    } finally {
      setIsSubmitting(false);
    }
  };

  const updateFeedback = async (id: string, payload: FeedbackUpdatePayload) => {
    setIsSubmitting(true);
    try {
      await FeedbackService.updateFeedback(id, payload);
      AlertService.success('Atualizado!', 'Sua edição foi enviada para moderação.');
      return true;
    } catch (error) {
      AlertService.error('Erro', error instanceof Error ? error.message : 'Não foi possível atualizar seu feedback.');
      return false;
    } finally {
      setIsSubmitting(false);
    }
  };

  return { sendFeedback, updateFeedback, isSubmitting };
}