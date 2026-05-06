import React from 'react';
import { useForm } from 'react-hook-form';
import styles from './PixPayment.module.scss';
import { usePixPayment } from '../hooks/usePixPayment';
import { Form, FormActions, FormInput, FormSubmit } from '@/components/layout/Form';

interface PixFormData {
  firstName: string;
  lastName: string;
  payerEmail: string;
  identificationNumber: string;
}

interface PixPaymentProps {
    planId: string;
    onPaymentComplete: (paymentId: string) => void; // NOVO
}

export const PixPayment: React.FC<PixPaymentProps> = ({ planId, onPaymentComplete }) => {
  const methods = useForm<PixFormData>({
    defaultValues: { identificationNumber: '', firstName: '', lastName: '', payerEmail: '' }
  });

  // Passamos onPaymentComplete para o hook
  const { loading, pixData, status, generatePix, copyToClipboard } = usePixPayment(planId, onPaymentComplete);

  const onSubmit = async (data: PixFormData) => {
    await generatePix({
      ...data,
      identificationType: 'CPF' 
    });
  };

  // Se o Pix foi gerado e está aguardando (Pending)[cite: 13, 19]
  if (pixData && status === 'pending') {
    return (
      <div className={styles.qrContainer}>
        <h3>Escaneie o QR Code para pagar</h3>
        <p>O pagamento é aprovado instantaneamente.</p>
        
        <div className={styles.qrImageWrapper}>
          <img 
            src={`data:image/png;base64,${pixData.qrCodeBase64}`} 
            alt="QR Code Pix" 
          />
        </div>

        <div className={styles.copyArea}>
          <input type="text" readOnly value={pixData.qrCode} />
          <button onClick={copyToClipboard}>Copiar Código Pix</button>
        </div>

        <p className={styles.expiry}>
          Expira em: {new Date(pixData.expirationDate).toLocaleTimeString()}
        </p>
      </div>
    );
  }

  return (
    <div className={styles.pixFormWrapper}>
      <header>
        <h3>Dados do Pagador</h3>
        <p>Precisamos desses dados para gerar seu Pix com segurança.</p>
      </header>

      <Form methods={methods} onSubmit={onSubmit} className={styles.pixForm}>
        <div className={styles.row}>
          <FormInput 
            name="firstName" 
            label="Nome" 
            placeholder="Ex: João"
            validation={{ required: 'O nome é obrigatório' }} 
            colSpan={6}
          />
          <FormInput 
            name="lastName" 
            label="Sobrenome" 
            placeholder="Ex: Silva"
            validation={{ required: 'O sobrenome é obrigatório' }} 
            colSpan={6}
          />
        </div>

        <FormInput 
          name="payerEmail" 
          label="E-mail para comprovante" 
          type="email"
          placeholder="seu@email.com"
          validation={{ 
            required: 'E-mail obrigatório',
            pattern: { value: /^\S+@\S+$/i, message: 'E-mail inválido' }
          }} 
        />

        <FormInput 
          name="identificationNumber" 
          label="CPF" 
          placeholder="000.000.000-00"
          validation={{ 
            required: 'CPF é obrigatório',
            minLength: { value: 11, message: 'CPF inválido' }
          }} 
        />

        <FormActions>
          <FormSubmit isLoading={loading} className={styles.generateBtn}>
            Gerar QR Code Pix
          </FormSubmit>
        </FormActions>
      </Form>
    </div>
  );
};