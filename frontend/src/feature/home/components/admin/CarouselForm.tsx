import React, { useState } from 'react';
import { GenericForm, type FormField } from '../../../../components/layout/Form/GenericForm';
import { HomeService } from '../../services/HomeService';
import type { CarouselContent, CarouselCreatePayload, CarouselUpdatePayload } from '../../types';
import './AdminForms.scss';
import { AlertService } from '@/core/alert/AlertService';
import { validateFileSize } from '@/core/utils/FileUploadWrapper';


interface CarouselFormProps {
  initialData?: CarouselContent;
  onSuccess?: () => void;
  onCancel?: () => void;
  useMock: boolean;
}

export const CarouselForm: React.FC<CarouselFormProps> = ({ initialData, onSuccess, onCancel, useMock }) => {
  const [isLoading, setIsLoading] = useState(false);

  const isEditing = !!initialData?.id;

  const fields: FormField<any>[] = [
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
      name: 'image',
      label: 'Imagem (10MB máx)',
      type: 'file',
      accept: 'image/jpeg, image/png, image/webp',
      validation: { required: isEditing ? false : 'Imagem é obrigatória' },
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

  const handleSubmit = async (data: any) => {
    setIsLoading(true);

    let file: File | undefined;
    if (data.image && data.image.length > 0) {
      file = data.image[0];
      if (!validateFileSize(file!, 10)) {
        setIsLoading(false);
        return;
      }
    }

    const dataPayload: CarouselCreatePayload = {
      title: data.title,
      order: Number(data.order),
      section: data.section
    };
    if (data.subtitle) dataPayload.subtitle = data.subtitle;
    if (data.action_url) dataPayload.action_url = data.action_url;

    try {
      if (isEditing && initialData?.id) {
        await HomeService.updateCarouselItem(initialData.id, dataPayload as CarouselUpdatePayload, useMock, file);
      } else {
        await HomeService.createCarouselItem(dataPayload, useMock, file);
      }
      
      if (onSuccess) {
        onSuccess();
      }
    } catch {
      AlertService.error('Erro', 'Falha ao salvar os dados. Tente novamente.');
    } finally {
      setIsLoading(false);
    }
  };

  const defaultValues = initialData ? {
    title: initialData.title,
    subtitle: initialData.subtitle || '',
    action_url: initialData.action_url || '',
    order: initialData.order,
    section: initialData.section
  } : {
    title: '',
    subtitle: '',
    action_url: '',
    order: 0,
    section: 'hero' as const
  };

  return (
    <div className="admin-form-container">
      <div className="form-header">
        <h3>{isEditing ? 'Editar Banner' : 'Novo Banner'}</h3>
      </div>
      <GenericForm<any>
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
