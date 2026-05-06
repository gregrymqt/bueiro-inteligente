import { useState, useEffect, useCallback } from 'react';
import { PlanService } from '../services/PlanService';
import type { PricingPlan } from '../types';
import { AlertService } from '@/core/alert/AlertService';

export function usePlanDetails(planId: string | null) {
  const [plan, setPlan] = useState<PricingPlan | null>(null);
  const [loading, setLoading] = useState<boolean>(true);

  const fetchPlan = useCallback(async () => {
    // Se não tiver ID (ex: url inválida no checkout), aborta e tira o loading
    if (!planId) {
      setLoading(false);
      return;
    }

    setLoading(true);
    try {
      const data = await PlanService.getPlanById(planId);
      setPlan(data);
    } catch (err) {
      AlertService.error('Erro ao buscar plano', err instanceof Error ? err.message : 'Plano não encontrado.');
      setPlan(null);
    } finally {
      setLoading(false);
    }
  }, [planId]);

  useEffect(() => {
    fetchPlan();
  }, [fetchPlan]);

  return { plan, loading, refetch: fetchPlan };
}