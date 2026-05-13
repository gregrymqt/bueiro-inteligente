// feature/home/components/CarouselForm.tsx
import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import type { CarouselSaveDto, CarouselResponse } from '../types/homeAdmin.types';
import { Form } from '@/components/layout/Form/Form';
import { useHomeAdmin } from '../hooks/useHomeAdmin';
import { ImageResolver } from '@/core/http/ImageResolver';

interface CarouselFormProps {
  initialData?: CarouselResponse;
  onSubmit: (data: CarouselSaveDto) => void;
  isLoading?: boolean;
}

export const CarouselForm: React.FC<CarouselFormProps> = ({ initialData, onSubmit, isLoading }) => {
  const [previewUrl, setPreviewUrl] = useState<string | null>(
    initialData?.image_url ? ImageResolver.resolve(initialData.image_url) : null
  );
  
  const { uploadImage, isUploading } = useHomeAdmin({ autoFetch: false });

  const methods = useForm<CarouselSaveDto>({
    defaultValues: initialData ? {
      title: initialData.title,
      subtitle: initialData.subtitle,
      section: initialData.section,
      order: initialData.order,
      action_url: initialData.action_url,
      upload_id: '' 
    } : {
      section: 'hero',
      order: 0,
      upload_id: ''
    }
  });

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const result = await uploadImage(file);
    
    if (result) {
      methods.setValue('upload_id', result.uploadId, { shouldValidate: true });
      if (result.uploadUrl) {
        setPreviewUrl(ImageResolver.resolve(result.uploadUrl));
      }
    }
  };

  // Intercepta e sanitiza os dados antes da submissão principal
  const handleSubmitInternal = (data: CarouselSaveDto) => {
    const sanitizedData: CarouselSaveDto = {
      ...data,
      // Converte string vazia para null, respeitando a validação [Url] do C#
      action_url: data.action_url?.trim() ? data.action_url.trim() : null,
      // Converte string vazia para null no subtítulo também
      subtitle: data.subtitle?.trim() ? data.subtitle.trim() : null,
      // Garante o envio do valor numérico
      order: Number(data.order)
    };

    onSubmit(sanitizedData);
  };

  return (
    <Form methods={methods} onSubmit={handleSubmitInternal}>
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

      <div style={{ gridColumn: 'span 12' }}>
        <label style={{ display: 'block', fontSize: '14px', fontWeight: 500, marginBottom: '4px' }}>
          Imagem do Slide
        </label>
        <input 
          type="file" 
          accept="image/*" 
          onChange={handleFileChange}
          disabled={isUploading}
          style={{ display: 'block', margin: '8px 0' }}
        />
        {isUploading && <p style={{ fontSize: '14px', color: '#6b7280' }}>Enviando arquivo e gerando UUID...</p>}
        {previewUrl && (
          <div style={{ marginTop: '1rem' }}>
            <img 
              src={previewUrl} 
              alt="Preview" 
              style={{ maxWidth: '100%', maxHeight: '200px', borderRadius: '8px', objectFit: 'cover' }} 
            />
          </div>
        )}
        
        <input 
          type="hidden" 
          {...methods.register('upload_id', { 
            required: initialData ? false : 'A seleção de uma imagem é obrigatória' 
          })} 
        />
        {methods.formState.errors.upload_id && (
          <span style={{ color: '#ef4444', fontSize: '14px', display: 'block', marginTop: '4px' }}>
            {methods.formState.errors.upload_id.message}
          </span>
        )}
      </div>

      <Form.Actions>
        <Form.Submit isLoading={isLoading || isUploading}>
          {initialData ? 'Salvar Alterações' : 'Criar Slide'}
        </Form.Submit>
      </Form.Actions>
    </Form>
  );
};