import React from 'react';
import { useForm } from 'react-hook-form';
import type { StatCardSaveDto, StatCardResponse } from '../types/homeAdmin.types';
import { Form } from '@/components/layout/Form/Form';

interface StatCardFormProps {
  initialData?: StatCardResponse;
  onSubmit: (data: StatCardSaveDto) => void;
  isLoading?: boolean;
}

export const StatCardForm: React.FC<StatCardFormProps> = ({ initialData, onSubmit, isLoading }) => {
  const methods = useForm<StatCardSaveDto>({
    defaultValues: initialData ?? {
      color: 'success',
      order: 0,
      icon_name: 'sensor'
    }
  });

  return (
    <Form methods={methods} onSubmit={onSubmit}>
      <Form.Input 
        name="title" 
        label="Título do Card" 
        placeholder="Ex: Bueiros Monitorados"
        validation={{ required: 'Campo obrigatório' }}
        colSpan={6}
      />

      <Form.Input 
        name="value" 
        label="Valor Exibido" 
        placeholder="Ex: 1,200 ou 95%"
        validation={{ required: 'Campo obrigatório' }}
        colSpan={6}
      />

      <Form.Textarea 
        name="description" 
        label="Descrição" 
        placeholder="Explique o que este número representa"
        validation={{ required: 'Campo obrigatório' }}
        colSpan={12}
      />

      <Form.Input 
        name="icon_name" 
        label="Nome do Ícone (Lucide)" 
        placeholder="Ex: sensor, cloud, bar-chart"
        validation={{ required: 'Informe o nome do ícone' }}
        colSpan={4}
      />

      <Form.Select 
        name="color" 
        label="Cor Visual" 
        colSpan={4}
        options={[
          { label: 'Verde (Sucesso)', value: 'success' },
          { label: 'Amarelo (Aviso)', value: 'warning' },
          { label: 'Vermelho (Perigo)', value: 'danger' }
        ]}
        validation={{ required: 'Selecione uma cor' }}
      />

      <Form.Input 
        name="order" 
        label="Ordem" 
        type="number"
        colSpan={4}
        validation={{ required: 'Defina a ordem' }}
      />

      <Form.Actions>
        <Form.Submit isLoading={isLoading}>
          {initialData ? 'Atualizar Card' : 'Adicionar Card'}
        </Form.Submit>
      </Form.Actions>
    </Form>
  );
};