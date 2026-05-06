import { useState, useCallback } from 'react';
import { PaymentMethodType } from '../types/checkout.type';
import { AlertService } from '@/core/alert/AlertService';

export const useCheckout = (planId: string | null) => {
  const [selectedMethod, setSelectedMethod] = useState<PaymentMethodType | null>(null);
  
  // NOVO: Estado que armazena o ID do pagamento quando a transação termina
  const [completedPaymentId, setCompletedPaymentId] = useState<string | null>(null);

  const handleSelect = useCallback((method: PaymentMethodType) => {
    if (!planId) {
      AlertService.error('Erro', 'Selecione um plano válido antes de prosseguir.');
      return;
    }
    setSelectedMethod(method);
    // Limpa qualquer status anterior se o usuário trocar de método
    setCompletedPaymentId(null); 
  }, [planId]);

  const resetSelection = useCallback(() => {
    setSelectedMethod(null);
    setCompletedPaymentId(null);
  }, []);

  // Função para os componentes de pagamento chamarem ao terminarem
  const handlePaymentComplete = useCallback((paymentId: string) => {
      setCompletedPaymentId(paymentId);
  }, []);

  return { 
    selectedMethod, 
    completedPaymentId,
    handleSelect, 
    resetSelection,
    handlePaymentComplete
  };
};