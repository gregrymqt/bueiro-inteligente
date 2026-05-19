import React, { useMemo, useCallback, useRef } from 'react';
import { CardPayment as MPCardPayment } from '@mercadopago/sdk-react';
import styles from './CardPayment.module.scss';
import { useCardPayment } from '../hooks/useCardPayment';
import type { ICardPaymentBrickPayer, ICardPaymentFormData } from '@mercadopago/sdk-react/esm/bricks/cardPayment/type';

interface CardPaymentProps {
  planId: string;
  amount: number;
  payerEmail?: string;
  onPaymentComplete: (paymentId: string) => void;
}

export const CardPayment: React.FC<CardPaymentProps> = ({ planId, amount, payerEmail, onPaymentComplete }) => {
  const { handleCardSubmit, status } = useCardPayment(planId, amount, onPaymentComplete);
  
  // 🛡️ SOLUÇÃO DEFINITIVA: Remoção do useState de e-mail.
  // Usamos um useRef apontando direto para o elemento do DOM. 
  // Digitar aqui gera ZERO re-renders na árvore de componentes.
  const emailInputRef = useRef<HTMLInputElement>(null);

  // ✅ Estabiliza o objeto de inicialização
  const initialization = useMemo(() => ({
    amount: amount,
    payer: {
      email: payerEmail || '',
    },
  }), [amount, payerEmail]);

  // ✅ Estabiliza o objeto de customização
  const customization = useMemo(() => ({
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
  }), []);

  // ✅ Memoriza o callback de envio extraindo o valor sincronamente do Ref no submit
  const onSubmit = useCallback(async (
    formData: ICardPaymentFormData<ICardPaymentBrickPayer>
  ): Promise<void> => {
    if (formData.payer && emailInputRef.current) {
      formData.payer.email = emailInputRef.current.value.trim();
    }
    await handleCardSubmit(formData);
  }, [handleCardSubmit]);

  // 🛡️ Estabiliza as funções de ciclo de vida do SDK para evitar quebras por novas referências de memória
  const handleReady = useCallback(() => {
    console.log('Mercado Pago Brick pronto e estável.');
  }, []);

  const handleError = useCallback((error: any) => {
    console.error('Erro detectado no Mercado Pago Brick:', error);
  }, []);

  return (
    <div className={styles.cardBrickContainer}>
      <header className={styles.header}>
        <h3>Pagamento com Cartão</h3>
        <p>Complete as etapas abaixo para ativar seu plano.</p>
      </header>

      <div className={styles.formSteps}>

        {/* PASSO 1: DADOS DE CONTATO */}
        <section className={styles.stepSection}>
          <div className={styles.stepHeader}>
            <span className={styles.stepBadge}>1</span>
            <h4>Dados de Contato</h4>
          </div>
          <div className={styles.stepContent}>
            <label className={styles.inputLabel} htmlFor="billing-email">
              E-mail para recebimento da nota/recibo
            </label>
            <input
              id="billing-email"
              ref={emailInputRef}
              className={styles.customInput}
              type="email"
              defaultValue={payerEmail ?? ''} // Inicializa o valor sem controlar o estado do ciclo de digitação
              placeholder="seuemail@dominio.com"
              required
            />
          </div>
        </section>

        <hr className={styles.divider} />

        {/* PASSO 2: DADOS DO CARTÃO (MERCADO PAGO BRICK) */}
        <section className={styles.stepSection}>
          <div className={styles.stepHeader}>
            <span className={styles.stepBadge}>2</span>
            <h4>Dados do Cartão</h4>
          </div>
          <div className={`${styles.stepContent} ${styles.brickWrapper}`}>
            <MPCardPayment
              initialization={initialization}
              customization={customization}
              onSubmit={onSubmit}
              onReady={handleReady} // Referência imutável garantida
              onError={handleError} // Referência imutável garantida
            />
          </div>
        </section>

      </div>

      {status === 'processing' && (
        <div className={styles.processingOverlay}>
          <div className={styles.spinner}></div>
          <p>Processando seu pagamento com segurança...</p>
        </div>
      )}
    </div>
  );
};