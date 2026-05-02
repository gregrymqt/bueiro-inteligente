import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Form } from '@/components/layout/Form';
import { useFeedbackForm } from '../../hooks/useFeedbackForm';
import type { Feedback, FeedbackCreatePayload } from '../../types';
import styles from './FeedbackForm.module.scss';

interface FeedbackFormProps {
  initialData?: Feedback; // Dados para edição[cite: 33]
  onSuccess?: () => void; // Callback para fechar modal/limpar estado
  onCancel?: () => void;
}

export const FeedbackForm: React.FC<FeedbackFormProps> = ({ initialData, onSuccess, onCancel }) => {
  const isEditing = Boolean(initialData?.id);
  const { sendFeedback, updateFeedback, isSubmitting } = useFeedbackForm(); // Hook com CRUD[cite: 37]
  
  const methods = useForm<FeedbackCreatePayload>({
    defaultValues: {
      rating: initialData?.rating ?? 5,
      comment: initialData?.comment ?? ''
    }
  });

  // Atualiza os campos se os dados iniciais mudarem[cite: 33]
  useEffect(() => {
    if (initialData) {
      methods.reset({
        rating: initialData.rating,
        comment: initialData.comment
      });
    }
  }, [initialData, methods]);

  const onSubmit = async (data: FeedbackCreatePayload) => {
    let success = false;

    if (isEditing && initialData) {
      success = await updateFeedback(initialData.id, data); // Chamada de UPDATE[cite: 37]
    } else {
      success = await sendFeedback(data); // Chamada de CREATE[cite: 37]
    }

    if (success) {
      methods.reset();
      onSuccess?.(); // Notifica o componente pai do sucesso
    }
  };

  return (
    <section className={styles.feedbackContainer}>
      <h3 className={styles.title}>
        {isEditing ? 'Editar sua avaliação' : 'Avalie sua experiência'}
      </h3>
      
      <Form methods={methods} onSubmit={onSubmit} className={styles.form}>
        <Form.Select
          name="rating"
          label="Sua Nota"
          validation={{ required: 'Por favor, selecione uma nota' }}
          options={[
            { label: '5 Estrelas - Excelente', value: 5 },
            { label: '4 Estrelas - Muito Bom', value: 4 },
            { label: '3 Estrelas - Bom', value: 3 },
            { label: '2 Estrelas - Regular', value: 2 },
            { label: '1 Estrela - Ruim', value: 1 },
          ]}
        />

        <Form.Textarea
          name="comment"
          label="Seu Comentário"
          placeholder="Conte-nos como o sistema está ajudando você..."
          rows={4}
          validation={{ 
            required: 'O comentário é obrigatório',
            minLength: { value: 10, message: 'Mínimo de 10 caracteres' }
          }}
        />

        <Form.Actions>
          {onCancel && (
            <button type="button" onClick={onCancel} className={styles.cancelBtn}>
              Cancelar
            </button>
          )}
          <Form.Submit isLoading={isSubmitting} className={styles.submitBtn}>
            {isEditing ? 'Salvar Alterações' : 'Enviar Avaliação'}
          </Form.Submit>
        </Form.Actions>
      </Form>
    </section>
  );
};