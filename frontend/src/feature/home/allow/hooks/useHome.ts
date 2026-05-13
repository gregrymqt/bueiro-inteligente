// feature/home/hooks/useHome.ts
import { useState, useCallback, useEffect } from 'react';
import { HomeService } from '../services/HomeService';
import { AlertService } from '@/core/alert/AlertService';
import { useActivePlans } from '@/feature/plan/hooks/useActivePlans';
import type { PricingPlan } from '@/feature/plan/types';
import type { CarouselResponse, StatCardResponse } from '../../admin/types/homeAdmin.types';

interface UseHomeResult {
  carousels: CarouselResponse[];
  stats: StatCardResponse[];
  plans: PricingPlan[];
  loading: boolean;
}

export function useHome(): UseHomeResult {
  const [carousels, setCarousels] = useState<CarouselResponse[]>([]);
  const [stats, setStats] = useState<StatCardResponse[]>([]);
  const [homeLoading, setHomeLoading] = useState(true);

  const { plans, loading: plansLoading } = useActivePlans();

  const fetchHomeData = useCallback(async () => {
    setHomeLoading(true);
    try {
      const data = await HomeService.getLandingPageData();
      // Ordena e armazena os slides e métricas baseados na propriedade 'order'
      setCarousels(data.carousels.sort((a, b) => a.order - b.order));
      setStats(data.stats.sort((a, b) => a.order - b.order));
    } catch {
      AlertService.error('Erro', 'Erro ao carregar dados da página inicial.');
      setCarousels([]);
      setStats([]);
    } finally {
      setHomeLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchHomeData();
  }, [fetchHomeData]);

  return { 
    carousels, 
    stats, 
    plans, 
    loading: homeLoading || plansLoading 
  };
}