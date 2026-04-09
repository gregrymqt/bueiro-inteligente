import { useState, useCallback, useEffect } from 'react';
import { HomeService } from '../services/HomeService';
import { AlertService } from '@/core/alert/AlertService';
import type { 
  CarouselContent, 
  StatCardContent, 
  CarouselCreatePayload, 
  CarouselUpdatePayload,
  StatCardCreatePayload,
  StatCardUpdatePayload
} from '../types';

export function useHomeAdmin() {
  const [carousels, setCarousels] = useState<CarouselContent[]>([]);
  const [stats, setStats] = useState<StatCardContent[]>([]);
  const [loading, setLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const fetchHomeData = useCallback(async () => {
    setLoading(true);
    try {
      const data = await HomeService.getHomeData();
      setCarousels(data.carousels || []);
      setStats(data.stats || []);
    } catch {
      AlertService.error('Erro', 'Falha ao carregar os dados da Home.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchHomeData();
  }, [fetchHomeData]);

  // ==========================================
  // Operações de Carousel
  // ==========================================

  const addBanner = async (payload: CarouselCreatePayload) => {
    setIsSaving(true);
    try {
      const newItem = await HomeService.createCarouselItem(payload);
      // Atualização otimista: insere o novo item na lista sem recarregar a página
      setCarousels((prev) => [...prev, newItem].sort((a, b) => a.order - b.order));
      AlertService.success('Banner criado com sucesso!');
      return true;
    } catch {
      AlertService.error('Erro', 'Não foi possível criar o banner.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const updateBanner = async (id: string, payload: CarouselUpdatePayload) => {
    setIsSaving(true);
    try {
      const updatedItem = await HomeService.updateCarouselItem(id, payload);
      // Atualização otimista
      setCarousels((prev) => 
        prev.map(item => item.id === id ? updatedItem : item).sort((a, b) => a.order - b.order)
      );
      AlertService.success('Banner atualizado com sucesso!');
      return true;
    } catch {
      AlertService.error('Erro', 'Não foi possível atualizar o banner.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const removeBanner = async (id: string) => {
    setIsSaving(true);
    try {
      await HomeService.deleteCarouselItem(id);
      // Atualização otimista: remove localmente o id deletado
      setCarousels((prev) => prev.filter(item => item.id !== id));
      AlertService.success('Banner excluído com sucesso!');
      return true;
    } catch {
      AlertService.error('Erro', 'Não foi possível remover o banner.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  // ==========================================
  // Operações de Estatísticas (Stats)
  // ==========================================

  const addStatCard = async (payload: StatCardCreatePayload) => {
    setIsSaving(true);
    try {
      const newStat = await HomeService.createStatCard(payload);
      setStats((prev) => [...prev, newStat].sort((a, b) => a.order - b.order));
      AlertService.success('Estatística criada com sucesso!');
      return true;
    } catch {
      AlertService.error('Erro', 'Não foi possível criar o card de estatística.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const updateStatCard = async (id: string, payload: StatCardUpdatePayload) => {
    setIsSaving(true);
    try {
      const updatedStat = await HomeService.updateStatCard(id, payload);
      setStats((prev) => 
        prev.map(stat => stat.id === id ? updatedStat : stat).sort((a, b) => a.order - b.order)
      );
      AlertService.success('Estatística atualizada com sucesso!');
      return true;
    } catch {
      AlertService.error('Erro', 'Não foi possível atualizar o card de estatística.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const removeStatCard = async (id: string) => {
    setIsSaving(true);
    try {
      await HomeService.deleteStatCard(id);
      setStats((prev) => prev.filter(stat => stat.id !== id));
      AlertService.success('Estatística excluída com sucesso!');
      return true;
    } catch {
      AlertService.error('Erro', 'Não foi possível remover o card de estatística.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  return {
    carousels,
    stats,
    loading,
    isSaving,
    refreshData: fetchHomeData,
    addBanner,
    updateBanner,
    removeBanner,
    addStatCard,
    updateStatCard,
    removeStatCard
  };
}