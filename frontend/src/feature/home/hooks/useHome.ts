import { useState, useCallback, useEffect } from 'react';
import { HomeService } from '../services/HomeService';
import { AlertService } from '@/core/alert/AlertService';
import { usePlans } from '../../plan/hooks/usePlans';
import type { HowItWorksStep, UserReview } from '../types';

interface UseHomeResult {
  steps: HowItWorksStep[];
  reviews: UserReview[];
  // Re-exportamos os planos e o loading combinado
  plans: ReturnType<typeof usePlans>['plans'];
  loading: boolean;
}

export function useHome(): UseHomeResult {
  const [steps, setSteps] = useState<HowItWorksStep[]>([]);
  const [reviews, setReviews] = useState<UserReview[]>([]);
  const [homeLoading, setHomeLoading] = useState(true);

  // Instanciamos o hook da feature de Planos
  const { plans, loading: plansLoading } = usePlans();

  const fetchHomeData = useCallback(async () => {
    setHomeLoading(true);
    try {
      const data = await HomeService.getLandingPageData();
      // O backend deve retornar uma estrutura com 'steps' e 'reviews' ordenadas
      setSteps(data.steps.sort((a, b) => a.order - b.order));
      setReviews(data.reviews);
    } catch {
      AlertService.error('Erro', 'Erro ao carregar dados da página inicial.');
      setSteps([]);
      setReviews([]);
    } finally {
      setHomeLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchHomeData();
  }, [fetchHomeData]);

  // O loading geral só acaba quando a Home E os Planos terminarem de carregar
  const combinedLoading = homeLoading || plansLoading;

  return { 
    steps, 
    reviews, 
    plans, 
    loading: combinedLoading 
  };
}