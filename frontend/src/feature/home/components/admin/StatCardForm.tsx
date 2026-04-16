import React, { useState } from 'react';
import { GenericForm, type FormField } from '../../../../components/layout/Form/GenericForm';
import { HomeService } from '../../services/HomeService';
import type { StatCardContent, StatCardCreatePayload } from '../../types';
import './AdminForms.scss';
import { AlertService } from '@/core/alert/AlertService';

interface StatCardFormProps {
  initialData?: StatCardContent;
  onSuccess?: () => void;
  onCancel?: () => void;
  useMock: boolean;
}

export const StatCardForm: React.FC<StatCardFormProps> = ({ initialData, onSuccess, onCancel, useMock }) => {
  const [isLoading, setIsLoading] = useState(false);

  const isEditing = !!initialData?.id;

  const fields: FormField<StatCardCreatePayload>[] = [
    {
      name: 'title',
      label: 'Título',
      type: 'text',
      validation: { required: 'Título é obrigatório' },
      colSpan: 6
    },
    {
      name: 'value',
      label: 'Valor',
      type: 'text',
      validation: { required: 'Valor é obrigatório' },
      colSpan: 6
    },
    {
      name: 'description',
      label: 'Descrição',
      type: 'text',
      validation: { required: 'Descrição é obrigatória' },
      colSpan: 12
    },
    {
      name: 'icon_name',
      label: 'Nome do Ícone (Lucide)',
      type: 'text',
      validation: { required: 'Nome do ícone é obrigatório' },
      placeholder: 'Ex: Activity, Droplet',
      colSpan: 4
    },
    {
      name: 'color',
      label: 'Cor',
      type: 'select',
      options: [
        { label: 'Sucesso (Verde)', value: 'success' },
        { label: 'Aviso (Amarelo)', value: 'warning' },
        { label: 'Perigo (Vermelho)', value: 'danger' }
      ],
      validation: { required: 'Cor é obrigatória' },
      colSpan: 4
    },
    {
      name: 'order',
      label: 'Ordem de Exibição',
      type: 'number',
      validation: { 
        required: 'Ordem é obrigatória',
        valueAsNumber: true 
      },
      colSpan: 4
    }
  ];

  const handleSubmit = async (data: StatCardCreatePayload) => {
    setIsLoading(true);

    // Garante que o tipo seja castado devidamente para update
    const payload = {
      ...data,
      order: Number(data.order)
    };

    try {
      if (isEditing && initialData.id) {
        await HomeService.updateStatCard(initialData.id, payload, useMock);
      } else {
        await HomeService.createStatCard(payload, useMock);
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
    value: initialData.value,
    description: initialData.description,
    icon_name: initialData.icon_name,
    color: initialData.color,
    order: initialData.order
  } : {
    title: '',
    value: '',
    description: '',
    icon_name: '',
    color: 'success' as const,
    order: 0
  };

  return (
    <div className="admin-form-container">
      <div className="form-header">
        <h3>{isEditing ? 'Editar Estatística' : 'Nova Estatística'}</h3>
      </div>
      <GenericForm<StatCardCreatePayload>
        fields={fields}
        onSubmit={handleSubmit}
        defaultValues={defaultValues}
        isLoading={isLoading}
        submitText={isEditing ? 'Atualizar Estatística' : 'Criar Estatística'}
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
