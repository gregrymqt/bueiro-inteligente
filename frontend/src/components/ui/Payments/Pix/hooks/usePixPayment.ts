import { useState, useEffect, useCallback } from 'react';
import { PixService } from '../services/PixService';
import { signalRClient } from '@/core/socket/SignalRClient';
import { AlertService } from '@/core/alert/AlertService';
import type { NotificationPayload } from '@/feature/notifications/hooks/useNotifications';
import type { CreatePixRequest, PixPaymentResponse } from '../types/pix.type';

export function usePixPayment(planId: string, onPaymentComplete: (paymentId: string) => void) {
  const [loading, setLoading] = useState(false);
  const [pixData, setPixData] = useState<PixPaymentResponse | null>(null);
  const [status, setStatus] = useState<'idle' | 'pending' | 'approved' | 'rejected'>('idle');

  const generatePix = async (payerInfo: Omit<CreatePixRequest, 'planId' | 'amount' | 'description'>) => {
    setLoading(true);
    try {
      const request: CreatePixRequest = {
        ...payerInfo,
        planId,
        amount: 0, 
        description: `Assinatura de Plano - ID: ${planId.substring(0, 8)}`,
      };

      const data = await PixService.createOrder(request);
      setPixData(data);
      setStatus('pending');
    } catch (err) {
      AlertService.error('Erro ao gerar Pix', err instanceof Error ? err.message : 'Não foi possível estabelecer conexão com o provedor de pagamento.');
    } finally {
      setLoading(false);
    }
  };

  // Escuta WebSocket
  useEffect(() => {
    if (!pixData || status !== 'pending') return;

    const unsubscribe = signalRClient.subscribe<NotificationPayload>('new_notification', (payload) => {
      const isMyTransaction = payload.message.includes(pixData.externalReference.substring(0, 8));

      if (isMyTransaction) {
        if (payload.type === 'Success') {
          setStatus('approved');
          onPaymentComplete(pixData.paymentId.toString()); // NOVO: Redireciona para o StatusScreen
        } else if (payload.type === 'Error') {
          setStatus('rejected');
          onPaymentComplete(pixData.paymentId.toString()); // NOVO
        }
      }
    });

    return () => unsubscribe();
  }, [pixData, status, onPaymentComplete]);

  // Função auxiliar para copiar código Pix (UX)
  const copyToClipboard = useCallback(() => {
    if (pixData?.qrCode) {
      navigator.clipboard.writeText(pixData.qrCode);
      AlertService.success('Copiado!', 'Código Pix copiado para a área de transferência.');
    }
  }, [pixData]);

  return {
    loading,
    pixData,
    status,
    generatePix,
    copyToClipboard
  };
}