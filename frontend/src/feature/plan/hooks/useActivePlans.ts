// feature/plan/hooks/useActivePlans.ts
import { useState, useEffect, useCallback } from 'react';
import { PlanService } from '../services/PlanService';
import type { PricingPlan } from '../types';
import { AlertService } from '@/core/alert/AlertService';

export function useActivePlans() {
  const [plans, setPlans] = useState<PricingPlan[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchActivePlans = useCallback(async () => {
    setLoading(true);
    try {
      const data = await PlanService.getActivePlans(); // Chamada pública
      setPlans(data);
    } catch (err) {
      // Tratamento de erro centralizado
      AlertService.error('Erro', err instanceof Error ? err.message : 'Erro desconhecido');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchActivePlans();
  }, [fetchActivePlans]);

  return { plans, loading, refetch: fetchActivePlans };
}