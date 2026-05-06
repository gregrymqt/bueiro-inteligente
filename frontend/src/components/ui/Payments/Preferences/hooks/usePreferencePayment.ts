import { useState } from 'react';
import { PreferenceService } from '../services/PreferenceService';
import { AlertService } from '@/core/alert/AlertService';
import type { CreatePreferenceRequest } from '../types/preference.types';

export function usePreferencePayment(planId: string) {
  const [loading, setLoading] = useState(false);

  /**
   * Cria a preferência no momento do clique. 
   * Retorna uma Promise resolvida com o ID da preferência, conforme exige o SDK.
   */
  const handleWalletSubmit = async (payerEmail: string): Promise<string> => {
    setLoading(true);

    try {
      const request: CreatePreferenceRequest = {
        title: 'Assinatura Bueiro Inteligente',
        description: `Plano ID: ${planId.substring(0, 8)}`,
        unitPrice: 0, // O backend ignora esse 0 e usa o preço do banco via PlanId para segurança[cite: 40]
        payerEmail: payerEmail || 'usuario@email.com',
        planId: planId
      };

      const response = await PreferenceService.createPreference(request);
      
      // O SDK do MP exige que o ID seja retornado nesta Promise
      return response.preferenceId; 

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Falha ao criar sessão de pagamento.';
      AlertService.error('Erro de Conexão', errorMessage);
      // Rejeita a promise para que o Brick saiba que falhou[cite: 39]
      throw err; 
    } finally {
      setLoading(false);
    }
  };

  return {
    loading,
    handleWalletSubmit
  };
}