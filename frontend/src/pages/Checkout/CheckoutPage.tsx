import React from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';

// Importa o teu hook seguro de autenticação (Resolve o TS 2339) 👇
import { useAuth } from '@/feature/auth/hooks/useAuth';

// Componentes de Pagamento
import { PixPayment } from '@/components/ui/Payments/Pix/components/PixPayment';
import { PreferencePayment } from '@/components/ui/Payments/Preferences/components/PreferencePayment';
import { CardPayment } from '@/components/ui/Payments/Card/components/CardPayment';

import { StatusScreen } from '@mercadopago/sdk-react';
import styles from './CheckoutPage.module.scss';
import { useCheckout } from '@/feature/checkout/hooks/useCheckout';
import { PaymentMethodType } from '@/feature/checkout/types/checkout.type';
import { usePlanDetails } from '@/feature/plan/hooks/usePlanDetails';

export const CheckoutPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const planId = searchParams.get('plan');
  const { plan, loading: loadingPlan } = usePlanDetails(planId);

  const {
    selectedMethod,
    completedPaymentId,
    handleSelect,
    resetSelection,
    handlePaymentComplete
  } = useCheckout(planId);

  // 1. Usa o useAuth em vez de useContext(AuthContext)
  const { user } = useAuth();

  // 2. Define os valores fixos por enquanto (Resolve o TS 2304)
  // Como o user do AuthContext pode ser null se o AuthProvider ainda estiver a carregar, usamos o optional chaining
  const payerEmail = user?.email || "email@fallback.com";

  // buscando o plano pelo `planId` para pegar o `amount` real do banco.
  const planAmount = plan?.price || 0; // Pega o preço real do backend!

  if (!planId) {
    return (
      <div className={styles.errorContainer}>
        <p>Plano não identificado ou inválido.</p>
        <button className={styles.backButton} onClick={() => navigate('/')}>
          Voltar para Início
        </button>
      </div>
    );
  }

  // -----------------------------------------------------
  // ETAPA 3: RENDERIZAR FEEDBACK DE STATUS (BRICK)
  // -----------------------------------------------------
  if (completedPaymentId) {
    return (
      <main className={styles.checkoutPage}>
        <section className={styles.paymentDetails} style={{ width: '100%', margin: '0 auto', maxWidth: '600px' }}>
          <StatusScreen
            initialization={{ paymentId: completedPaymentId }}
            customization={{
              visual: {
                showExternalReference: true,
                style: {
                  theme: 'default', // Puxa do MP, mas você pode usar 'dark' se quiser
                }
              },
              backUrls: {
                error: `${window.location.origin}/checkout?plan=${planId}`, // Tentar de novo
                return: `${window.location.origin}/dashboard` // Ir pra home/dashboard
              }
            }}
            onReady={() => console.log('Status Screen carregado')}
            onError={(error) => console.error('Erro no Status Screen', error)}
          />
        </section>
      </main>
    );
  }

  // -----------------------------------------------------
  // ETAPA 1 E 2: SELEÇÃO E PAGAMENTO
  // -----------------------------------------------------
  return (
    <main className={styles.checkoutPage}>
      <header className={styles.header}>
        <button
          className={styles.backButton}
          onClick={() => selectedMethod ? resetSelection() : navigate('/')}
        >
          &larr; Voltar
        </button>
        <h1 className={styles.title}>Finalizar Assinatura</h1>
        <p className={styles.subtitle}>Escolha como deseja ativar o seu plano de monitorização.</p>
      </header>

      <section className={styles.content}>
        {/* Seletor de Métodos (Esquerda no Desktop) */}
        <div className={`${styles.methodSelector} ${selectedMethod ? styles.hiddenOnMobile : ''}`}>
          <h2 className={styles.sectionTitle}>Forma de Pagamento</h2>

          <div className={styles.gridOptions}>
            <div
              className={`${styles.optionCard} ${selectedMethod === PaymentMethodType.PIX ? styles.active : ''}`}
              onClick={() => handleSelect(PaymentMethodType.PIX)}
            >
              <span className={styles.badge}>Instantâneo</span>
              <div className={styles.iconWrapper}>⚡</div>
              <div className={styles.optionInfo}>
                <span className={styles.optionTitle}>Pix</span>
                <span className={styles.optionDesc}>QR Code ou Copia e Cola</span>
              </div>
            </div>

            <div
              className={`${styles.optionCard} ${selectedMethod === PaymentMethodType.CREDIT_CARD ? styles.active : ''}`}
              onClick={() => handleSelect(PaymentMethodType.CREDIT_CARD)}
            >
              <div className={styles.iconWrapper}>💳</div>
              <div className={styles.optionInfo}>
                <span className={styles.optionTitle}>Cartão de Crédito</span>
                <span className={styles.optionDesc}>Até 12x com aprovação na hora</span>
              </div>
            </div>

            <div
              className={`${styles.optionCard} ${selectedMethod === PaymentMethodType.MERCADO_PAGO ? styles.active : ''}`}
              onClick={() => handleSelect(PaymentMethodType.MERCADO_PAGO)}
            >
              <div className={styles.iconWrapper}>💙</div>
              <div className={styles.optionInfo}>
                <span className={styles.optionTitle}>Mercado Pago</span>
                <span className={styles.optionDesc}>Saldo, cartões salvos ou crédito</span>
              </div>
            </div>
          </div>
        </div>

        {/* Detalhes do Pagamento / Bricks (Direita no Desktop) */}
        <div className={styles.paymentDetails}>
          {selectedMethod === PaymentMethodType.PIX && (
            <PixPayment
              planId={planId}
              onPaymentComplete={handlePaymentComplete}
            />
          )}

          {selectedMethod === PaymentMethodType.CREDIT_CARD && (
            <CardPayment
              planId={planId}
              amount={planAmount}
              payerEmail={payerEmail}
              onPaymentComplete={handlePaymentComplete}
            />
          )}

          {selectedMethod === PaymentMethodType.MERCADO_PAGO && (
            <PreferencePayment
              planId={planId}
              payerEmail={payerEmail}
            // Preferences geralmente redireciona (redirectMode: 'self'),
            // então pode não precisar da callback de complete aqui,
            // mas a mantemos caso mude o redirectMode no futuro.
            />
          )}

          {!selectedMethod && (
            <div className={styles.emptyState}>
              <p>Selecione uma forma de pagamento à esquerda para continuar.</p>
            </div>
          )}
        </div>
      </section>
    </main>
  );
};