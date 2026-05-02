import { useState, useCallback, useEffect } from 'react';
import { AlertService } from '@/core/alert/AlertService';
import type { PricingPlan } from '../types';
import { PlanService } from '../services/PlanService';

export function usePlans() {
  const [plans, setPlans] = useState<PricingPlan[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchPlans = useCallback(async () => {
    setLoading(true);
    try {
      const data = await PlanService.getPlans();
      setPlans(data);
    } catch {
      // Usando AlertService em vez de window.alert, conforme as regras
      AlertService.error('Erro', 'Erro ao carregar os planos.');
      setPlans([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchPlans();
  }, [fetchPlans]);

  return { plans, loading, refreshPlans: fetchPlans };
}