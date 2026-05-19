import { useState, useEffect, useCallback } from 'react'; // 🆕 Importado o useCallback
import { CardService } from '../services/CardService';
import { signalRClient } from '@/core/socket/SignalRClient';
import { AlertService } from '@/core/alert/AlertService';
import type { CreditCardRequest, CreditCardResponse } from '../types/card.types';
import type { NotificationPayload } from '@/feature/notifications/hooks/useNotifications';
import type { ICardPaymentBrickPayer, ICardPaymentFormData } from '@mercadopago/sdk-react/esm/bricks/cardPayment/type';

export function useCardPayment(planId: string, amount: number, onPaymentComplete: (paymentId: string) => void) {
  const [loading, setLoading] = useState(false);
  const [paymentResult, setPaymentResult] = useState<CreditCardResponse | null>(null);
  const [status, setStatus] = useState<'idle' | 'processing' | 'success' | 'failure' | '3ds_required'>('idle');

  // Altere apenas o bloco correspondente dentro do método handleCardSubmit:

  const handleCardSubmit = useCallback(async (formData: ICardPaymentFormData<ICardPaymentBrickPayer>) => {
    setLoading(true);
    setStatus('processing');

    try {
      // 🛡️ Mapeamento seguro extraindo os dados reais do formulário do Brick
      const request: CreditCardRequest = {
        token: formData.token,
        paymentMethodId: formData.payment_method_id,
        installments: formData.installments,
        payerEmail: formData.payer.email || '',

        // O SDK do MP divide o nome completo do titular nestas duas propriedades internas
        first_name: (formData.payer as any).first_name || '',
        last_name: (formData.payer as any).last_name || '',

        // Captura o tipo de documento (CPF) e o número preenchidos na UI
        identificationType: formData.payer.identification?.type,
        identificationNumber: formData.payer.identification?.number,

        description: `Assinatura de Plano - ID: ${planId.substring(0, 8)}`,
        amount: amount,
        planId: planId
      };

      const response = await CardService.processPayment(request);
      setPaymentResult(response);

      if (response.externalResourceUrl) {
        setStatus('3ds_required');
        window.location.href = response.externalResourceUrl;
        return;
      }

      if (response.status === 'approved' || response.status === 'processed') {
        setStatus('success');
        onPaymentComplete(response.paymentId.toString());
      } else {
        setStatus('failure');
        onPaymentComplete(response.paymentId.toString());
        AlertService.error('Pagamento Recusado', response.statusDetail || 'Verifique os dados do cartão.');
      }
    } catch (err) {
      setStatus('failure');
      const errorMessage = err instanceof Error ? err.message : 'Erro interno ao processar cartão.';
      AlertService.error('Erro no Processamento', errorMessage);
    } finally {
      setLoading(false);
    }
  }, [planId, amount, onPaymentComplete]);

  // Escuta WebSocket
  useEffect(() => {
    if (!paymentResult || status !== 'processing') return;

    const unsubscribe = signalRClient.subscribe<NotificationPayload>('new_notification', (payload) => {
      const isMyTransaction = payload.message.includes(paymentResult.externalReference.substring(0, 8));

      if (isMyTransaction) {
        if (payload.type === 'Success') {
          setStatus('success');
          onPaymentComplete(paymentResult.paymentId.toString());
        } else if (payload.type === 'Error') {
          setStatus('failure');
          onPaymentComplete(paymentResult.paymentId.toString());
        }
      }
    });

    return () => unsubscribe();
  }, [paymentResult, status, onPaymentComplete]);

  return { loading, status, paymentResult, handleCardSubmit };
}