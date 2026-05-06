import { useState, useEffect } from 'react';
import { CardService } from '../services/CardService';
import { signalRClient } from '@/core/socket/SignalRClient';
import { AlertService } from '@/core/alert/AlertService';
import type { CreditCardRequest, CreditCardResponse } from '../types/card.types';
import type { NotificationPayload } from '@/feature/notifications/hooks/useNotifications';
import type { ICardPaymentBrickPayer, ICardPaymentFormData } from '@mercadopago/sdk-react/esm/bricks/cardPayment/type';

// NOVO: Adicionado onPaymentComplete como parâmetro
export function useCardPayment(planId: string, onPaymentComplete: (paymentId: string) => void) {
  const [loading, setLoading] = useState(false);
  const [paymentResult, setPaymentResult] = useState<CreditCardResponse | null>(null);
  const [status, setStatus] = useState<'idle' | 'processing' | 'success' | 'failure' | '3ds_required'>('idle');

  const handleCardSubmit = async (formData: ICardPaymentFormData<ICardPaymentBrickPayer>) => {
    setLoading(true);
    setStatus('processing');

    try {
      const request: CreditCardRequest = {
        token: formData.token,
        paymentMethodId: formData.payment_method_id,
        installments: formData.installments,
        payerEmail: formData.payer.email || '',
        description: `Assinatura de Plano - ID: ${planId.substring(0, 8)}`,
        amount: 0,
        planId: planId
      };

      const response = await CardService.processPayment(request);
      setPaymentResult(response);

      if (response.externalResourceUrl) {
        setStatus('3ds_required');
        window.location.href = response.externalResourceUrl;
        return;
      }

      // Se já vier aprovado/processado direto do backend
      if (response.status === 'approved' || response.status === 'processed') {
        setStatus('success');
        onPaymentComplete(response.paymentId.toString()); // NOVO
      } else {
        setStatus('failure');
        onPaymentComplete(response.paymentId.toString()); // NOVO (para exibir erro no StatusScreen)
        AlertService.error('Pagamento Recusado', response.statusDetail || 'Verifique os dados do cartão.');
      }
    } catch (err) {
      setStatus('failure');
      const errorMessage = err instanceof Error ? err.message : 'Erro interno ao processar cartão.';
      AlertService.error('Erro no Processamento', errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Escuta WebSocket
  useEffect(() => {
    if (!paymentResult || status !== 'processing') return;

    const unsubscribe = signalRClient.subscribe<NotificationPayload>('new_notification', (payload) => {
      const isMyTransaction = payload.message.includes(paymentResult.externalReference.substring(0, 8));

      if (isMyTransaction) {
        if (payload.type === 'Success') {
          setStatus('success');
          onPaymentComplete(paymentResult.paymentId.toString()); // NOVO
        } else if (payload.type === 'Error') {
          setStatus('failure');
          onPaymentComplete(paymentResult.paymentId.toString()); // NOVO
        }
      }
    });

    return () => unsubscribe();
  }, [paymentResult, status, onPaymentComplete]);

  return { loading, status, paymentResult, handleCardSubmit };
}