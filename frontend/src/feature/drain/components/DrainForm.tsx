import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Form } from '@/components/layout/Form';
import { Button } from '@/components/ui/Button/Button';
import type { Drain, DrainCreatePayload } from '../types';
import styles from './DrainForm.module.scss';

interface DrainFormValues {
  name: string;
  address: string;
  latitude: number | '';
  longitude: number | '';
  hardware_id: string;
  is_active: string;
}

interface DrainFormProps {
  initialData?: Drain;
  onSubmit: (payload: DrainCreatePayload) => Promise<void> | void;
  onCancel?: () => void;
  isLoading?: boolean;
}

const getDefaultValues = (initialData?: Drain): DrainFormValues => {
  return {
    name: initialData?.name ?? '',
    address: initialData?.address ?? '',
    latitude: initialData?.latitude ?? '',
    longitude: initialData?.longitude ?? '',
    hardware_id: initialData?.hardware_id ?? '',
    is_active: initialData ? String(initialData.is_active) : 'true',
  };
};

export const DrainForm = ({ initialData, onSubmit, onCancel, isLoading = false }: DrainFormProps) => {
  const isEditing = Boolean(initialData?.id);
  const methods = useForm<DrainFormValues>({
    defaultValues: getDefaultValues(initialData),
    mode: 'onSubmit',
  });

  useEffect(() => {
    methods.reset(getDefaultValues(initialData));
  }, [initialData, methods]);

  const handleSubmit = async (values: DrainFormValues): Promise<void> => {
    const latitude = typeof values.latitude === 'number' ? values.latitude : Number(values.latitude);
    const longitude = typeof values.longitude === 'number' ? values.longitude : Number(values.longitude);

    await onSubmit({
      name: values.name.trim(),
      address: values.address.trim(),
      latitude,
      longitude,
      hardware_id: values.hardware_id.trim(),
      is_active: values.is_active === 'true',
    });
  };

  return (
    <section className={styles.card}>
      <div className={styles.header}>
        <p className={styles.eyebrow}>{isEditing ? 'Edição administrativa' : 'Novo cadastro'}</p>
        <h2 className={styles.title}>{isEditing ? 'Editar bueiro' : 'Cadastrar bueiro'}</h2>
        <p className={styles.description}>
          Mantenha os dados cadastrais do drain sincronizados com o hardware em campo.
        </p>
      </div>

      <Form methods={methods} onSubmit={handleSubmit} className={styles.form}>
        <div className={styles.fieldsGrid}>
          <Form.Input
            name="name"
            label="Nome do bueiro"
            placeholder="Ex: Bueiro da Avenida Central"
            validation={{ required: 'Nome do bueiro é obrigatório' }}
            colSpan={6}
          />

          <Form.Input
            name="hardware_id"
            label="ID do hardware"
            placeholder="Ex: ESP32-001"
            validation={{ required: 'ID do hardware é obrigatório' }}
            colSpan={6}
          />

          <Form.Input
            name="address"
            label="Endereço"
            placeholder="Rua, número e referência"
            validation={{ required: 'Endereço é obrigatório' }}
            colSpan={12}
          />

          <Form.Input
            name="latitude"
            label="Latitude"
            type="number"
            step="any"
            inputMode="decimal"
            placeholder="-23.550520"
            validation={{
              required: 'Latitude é obrigatória',
              valueAsNumber: true,
              validate: (value: number | '') => (value !== '' && !Number.isNaN(value) ? true : 'Latitude é obrigatória'),
            }}
            colSpan={4}
          />

          <Form.Input
            name="longitude"
            label="Longitude"
            type="number"
            step="any"
            inputMode="decimal"
            placeholder="-46.633308"
            validation={{
              required: 'Longitude é obrigatória',
              valueAsNumber: true,
              validate: (value: number | '') => (value !== '' && !Number.isNaN(value) ? true : 'Longitude é obrigatória'),
            }}
            colSpan={4}
          />

          <Form.Select
            name="is_active"
            label="Status do bueiro"
            options={[
              { label: 'Ativo', value: 'true' },
              { label: 'Inativo', value: 'false' },
            ]}
            validation={{ required: 'Status do bueiro é obrigatório' }}
            colSpan={4}
          />
        </div>

        <Form.Actions className={styles.actions}>
          {onCancel ? (
            <Button type="button" variant="secondary" onClick={onCancel} disabled={isLoading} className={styles.cancelButton}>
              Cancelar
            </Button>
          ) : null}

          <Button type="submit" isLoading={isLoading} className={styles.submitButton}>
            {isEditing ? 'Salvar alterações' : 'Criar bueiro'}
          </Button>
        </Form.Actions>
      </Form>
    </section>
  );
};