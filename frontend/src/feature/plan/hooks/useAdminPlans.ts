// feature/plan/hooks/useAdminPlans.ts
import { useState, useEffect, useCallback } from 'react';
import { PlanService } from '../services/PlanService';
import { AlertService } from '@/core/alert/AlertService';
import type { 
  PricingPlan, 
  PricingPlanCreatePayload, 
  PricingPlanUpdatePayload 
} from '../types';

export function useAdminPlans() {
  const [plans, setPlans] = useState<PricingPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Busca a lista completa para o Admin
  const fetchAllPlans = useCallback(async () => {
    setLoading(true);
    try {
      const data = await PlanService.getAllPlans();
      setPlans(data);
    } catch (err) {
      AlertService.error('Erro', 'Falha ao carregar a lista administrativa de planos.');
    } finally {
      setLoading(false);
    }
  }, []);

  // Criação integrada ao Mercado Pago
  const addPlan = async (payload: PricingPlanCreatePayload) => {
    setIsSubmitting(true);
    try {
      await PlanService.createPlan(payload);
      AlertService.success('Sucesso', 'Novo plano criado e integrado ao Mercado Pago.');
      await fetchAllPlans(); // Atualiza a lista
      return true;
    } catch (err) {
      AlertService.error('Erro', 'Não foi possível criar o plano no gateway de pagamento.');
      return false;
    } finally {
      setIsSubmitting(false);
    }
  };

  // Atualização de dados visuais e preço
  const editPlan = async (id: string, payload: PricingPlanUpdatePayload) => {
    setIsSubmitting(true);
    try {
      await PlanService.updatePlan(id, payload);
      AlertService.success('Sucesso', 'Informações do plano atualizadas com sucesso.');
      await fetchAllPlans();
      return true;
    } catch (err) {
      AlertService.error('Erro', 'Erro ao atualizar o plano.');
      return false;
    } finally {
      setIsSubmitting(false);
    }
  };

  // Inativação lógica (Toggle Status)
  const toggleStatus = async (id: string, currentStatus: 'active' | 'inactive') => {
    const newStatus = currentStatus === 'active' ? 'inactive' : 'active';
    try {
      await PlanService.updatePlanStatus(id, newStatus);
      AlertService.success('Status Atualizado', `O plano agora está ${newStatus === 'active' ? 'Ativo' : 'Inativo'}.`);
      await fetchAllPlans();
    } catch (err) {
      AlertService.error('Erro', 'Falha ao alterar a visibilidade do plano.');
    }
  };

  useEffect(() => {
    fetchAllPlans();
  }, [fetchAllPlans]);

  return {
    plans,
    loading,
    isSubmitting,
    addPlan,
    editPlan,
    toggleStatus,
    refresh: fetchAllPlans
  };
}