import React from 'react';
import { CardPayment as MPCardPayment } from '@mercadopago/sdk-react';
import styles from './CardPayment.module.scss';
import { useCardPayment } from '../hooks/useCardPayment';
// Tipos nativos do SDK[cite: 34, 38]
import type { ICardPaymentBrickPayer, ICardPaymentFormData, IAdditionalData } from '@mercadopago/sdk-react/esm/bricks/cardPayment/type';

interface CardPaymentProps {
  planId: string;
  amount: number;
  payerEmail?: string;
  onPaymentComplete: (paymentId: string) => void; // NOVO
}

export const CardPayment: React.FC<CardPaymentProps> = ({ planId, amount, payerEmail, onPaymentComplete }) => {
  // Passamos onPaymentComplete para o hook
  const { handleCardSubmit, status } = useCardPayment(planId, onPaymentComplete);

  const initialization = {
    amount: amount,
    payer: {
      email: payerEmail || '',
    },
  };

  const customization = {
    visual: {
      style: {
        theme: 'default' as const,
        customVariables: {
          baseColor: '#0b5fb4',
          formBackgroundColor: '#ffffff',
          borderRadiusMedium: '0.75rem',
        },
      },
    },
    texts: {
      formSubmit: 'Confirmar Assinatura',
    },
  };

  /**
   * Assinatura idêntica ao contrato exigido pela prop 'onSubmit'[cite: 34, 38]
   */
  const onSubmit = async (
    formData: ICardPaymentFormData<ICardPaymentBrickPayer>,
    _additionalData?: IAdditionalData
  ): Promise<void> => {
    await handleCardSubmit(formData);
  };

  const onError = (error: unknown): void => {
    console.error('Erro no Card Brick:', error);
  };

  return (
    <div className={styles.cardBrickContainer}>
      <header className={styles.header}>
        <h3>Pagamento com Cartão</h3>
        <p>Insira os dados do seu cartão para ativar o plano.</p>
      </header>

      <div className={styles.brickWrapper}>
        <MPCardPayment
          initialization={initialization}
          customization={customization}
          onSubmit={onSubmit}
          onReady={() => { }}
          onError={onError}
        />
      </div>

      {status === 'processing' && (
        <div className={styles.processingOverlay}>
          <div className={styles.spinner}></div>
          <span>Validando transação...</span>
        </div>
      )}
    </div>
  );
};
