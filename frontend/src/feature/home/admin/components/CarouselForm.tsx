import React from 'react';
import { useForm } from 'react-hook-form';
import type { CarouselSaveDto, CarouselResponse } from '../types/homeAdmin.types';
import { Form } from '@/components/layout/Form/Form';

interface CarouselFormProps {
  initialData?: CarouselResponse;
  onSubmit: (data: CarouselSaveDto) => void;
  isLoading?: boolean;
}

export const CarouselForm: React.FC<CarouselFormProps> = ({ initialData, onSubmit, isLoading }) => {
  const methods = useForm<CarouselSaveDto>({
    defaultValues: initialData ? {
      title: initialData.title,
      subtitle: initialData.subtitle,
      section: initialData.section,
      order: initialData.order,
      action_url: initialData.action_url,
      // upload_id viria de um componente de upload, mas mantemos o valor se for edição
    } : {
      section: 'hero',
      order: 0
    }
  });

  return (
    <Form methods={methods} onSubmit={onSubmit}>
      <Form.Input 
        name="title" 
        label="Título do Slide" 
        placeholder="Ex: Proteja sua cidade..."
        validation={{ required: 'O título é obrigatório' }}
        colSpan={12}
      />

      <Form.Input 
        name="subtitle" 
        label="Subtítulo" 
        placeholder="Breve descrição do slide"
        colSpan={12}
      />

      <Form.Select 
        name="section" 
        label="Seção" 
        colSpan={6}
        options={[
          { label: 'Hero (Destaque Principal)', value: 'hero' },
          { label: 'Alertas', value: 'alerts' },
          { label: 'Estatísticas', value: 'stats' }
        ]}
        validation={{ required: 'Selecione uma seção' }}
      />

      <Form.Input 
        name="order" 
        label="Ordem de Exibição" 
        type="number"
        colSpan={6}
        validation={{ required: 'Defina a ordem' }}
      />

      <Form.Input 
        name="action_url" 
        label="URL de Ação (Botão)" 
        placeholder="https://..."
        colSpan={12}
      />

      <Form.Input 
        name="upload_id" 
        label="ID do Upload (Imagem)" 
        placeholder="UUID da imagem no sistema"
        validation={{ required: 'A imagem é obrigatória' }}
        colSpan={12}
      />

      <Form.Actions>
        <Form.Submit isLoading={isLoading}>
          {initialData ? 'Salvar Alterações' : 'Criar Slide'}
        </Form.Submit>
      </Form.Actions>
    </Form>
  );
};