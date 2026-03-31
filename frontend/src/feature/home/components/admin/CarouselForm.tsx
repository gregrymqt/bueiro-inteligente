import React, { useState } from 'react';
import { GenericForm, type FormField } from '../../../../components/layout/Form/GenericForm';
import  { HomeService } from '../../services/HomeService';
import type { CarouselContent, CarouselCreatePayload } from '../../types';import './AdminForms.scss';
interface CarouselFormProps {
  initialData?: CarouselContent;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export const CarouselForm: React.FC<CarouselFormProps> = ({ initialData, onSuccess, onCancel }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const isEditing = !!initialData?.id;

  const fields: FormField<CarouselCreatePayload>[] = [
    {
      name: 'title',
      label: 'Título',
      type: 'text',
      validation: { required: 'Título é obrigatório' },
      colSpan: 12
    },
    {
      name: 'subtitle',
      label: 'Subtítulo',
      type: 'text',
      colSpan: 12
    },
    {
      name: 'image_url',
      label: 'URL da Imagem',
      type: 'text',
      validation: { required: 'URL da imagem é obrigatória' },
      colSpan: 12
    },
    {
      name: 'action_url',
      label: 'URL de Ação (Acesso ao clicar)',
      type: 'text',
      colSpan: 12
    },
    {
      name: 'order',
      label: 'Ordem de Exibição',
      type: 'number',
      validation: { 
        required: 'Ordem é obrigatória',
        valueAsNumber: true
      },
      colSpan: 6
    },
    {
      name: 'section',
      label: 'Seção',
      type: 'select',
      options: [
        { label: 'Hero', value: 'hero' },
        { label: 'Alertas', value: 'alerts' },
        { label: 'Estatísticas', value: 'stats' }
      ],
      validation: { required: 'Seção é obrigatória' },
      colSpan: 6
    }
  ];

  const handleSubmit = async (data: CarouselCreatePayload) => {
    setIsLoading(true);
    setErrorMsg(null);

    // Converte order para número caso chegue como string
    const payload = {
      ...data,
      order: Number(data.order)
    };

    try {
      if (isEditing && initialData.id) {
        await HomeService.updateCarouselItem(initialData.id, payload);
      } else {
        await HomeService.createCarouselItem(payload);
      }
      
      if (onSuccess) {
        onSuccess();
      }
    } catch (error) {
      console.error('Erro ao salvar carousel:', error);
      setErrorMsg('Falha ao salvar os dados. Tente novamente.');
    } finally {
      setIsLoading(false);
    }
  };

  const defaultValues = initialData ? {
    title: initialData.title,
    subtitle: initialData.subtitle || '',
    image_url: initialData.image_url,
    action_url: initialData.action_url || '',
    order: initialData.order,
    section: initialData.section
  } : {
    title: '',
    subtitle: '',
    image_url: '',
    action_url: '',
    order: 0,
    section: 'hero' as const
  };

  return (
    <div className="admin-form-container">
      <div className="form-header">
        <h3>{isEditing ? 'Editar Banner' : 'Novo Banner'}</h3>
      </div>
      {errorMsg && (
        <div className="form-error-message">
          {errorMsg}
        </div>
      )}
      <GenericForm<CarouselCreatePayload>
        fields={fields}
        onSubmit={handleSubmit}
        defaultValues={defaultValues}
        isLoading={isLoading}
        submitText={isEditing ? 'Atualizar Banner' : 'Criar Banner'}
      >
        <div className="form-actions">
          {onCancel && (
            <button 
              type="button" 
              onClick={onCancel} 
              disabled={isLoading}
              className="btn-cancel"
            >
              Cancelar
            </button>
          )}
        </div>
      </GenericForm>
    </div>
  );
};
