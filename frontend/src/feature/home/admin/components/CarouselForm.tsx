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
    initialData?.desktop_image_url ? ImageResolver.resolve(initialData.desktop_image_url) : null
  );
  
  const { uploadImage, isUploading } = useHomeAdmin({ autoFetch: false });

  const methods = useForm<CarouselSaveDto>({
    defaultValues: initialData ? {
      title: initialData.title,
      subtitle: initialData.subtitle,
      section: initialData.section,
      order: initialData.order,
      action_url: initialData.action_url,
      desktop_upload_id: '',
      mobile_upload_id: ''
    } : {
      section: 'hero',
      order: 0,
      desktop_upload_id: '',
      mobile_upload_id: ''
    }
  });

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const result = await uploadImage(file);
    
    if (result) {
      methods.setValue('desktop_upload_id', result.desktopId, { shouldValidate: true });
      methods.setValue('mobile_upload_id', result.mobileId, { shouldValidate: true });
      if (result.desktopUrl) {
        setPreviewUrl(ImageResolver.resolve(result.desktopUrl));
      }
    }
  };

  const handleSubmitInternal = (data: CarouselSaveDto) => {
    // Clona o objeto e sanitiza as strings
    const sanitizedData: CarouselSaveDto = {
      ...data,
      action_url: data.action_url?.trim() ? data.action_url.trim() : null,
      subtitle: data.subtitle?.trim() ? data.subtitle.trim() : null,
      order: Number(data.order)
    };

    // O truque: Se for edição (initialData existe) e não houve upload novo (id vazio),
    // nós deletamos a chave do payload. O C# vai receber como null e ignorar o Update da imagem!
    if (initialData) {
      if (!sanitizedData.desktop_upload_id) {
        delete sanitizedData.desktop_upload_id;
      }
      if (!sanitizedData.mobile_upload_id) {
        delete sanitizedData.mobile_upload_id;
      }
    }

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
          {...methods.register('desktop_upload_id', { 
            required: initialData ? false : 'A seleção de uma imagem é obrigatória' 
          })} 
        />
        <input 
          type="hidden" 
          {...methods.register('mobile_upload_id')} 
        />
        {methods.formState.errors.desktop_upload_id && (
          <span style={{ color: '#ef4444', fontSize: '14px', display: 'block', marginTop: '4px' }}>
            {methods.formState.errors.desktop_upload_id.message}
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