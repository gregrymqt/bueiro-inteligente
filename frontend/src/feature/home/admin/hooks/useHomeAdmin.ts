// feature/home/hooks/useHomeAdmin.ts
import { useState, useCallback, useEffect } from 'react';
import { HomeAdminService } from '../services/HomeAdminService';
import { AlertService } from '@/core/alert/AlertService';
import type { 
  CarouselResponse, 
  StatCardResponse, 
  CarouselSaveDto, 
  StatCardSaveDto 
} from '../types/homeAdmin.types';

interface UseHomeAdminOptions {
  autoFetch?: boolean;
}

export function useHomeAdmin(options: UseHomeAdminOptions = { autoFetch: true }) {
  const [carousels, setCarousels] = useState<CarouselResponse[]>([]);
  const [stats, setStats] = useState<StatCardResponse[]>([]);
  const [loading, setLoading] = useState(options.autoFetch ? true : false);
  const [isUploading, setIsUploading] = useState(false);

  const fetchAdminData = useCallback(async () => {
    setLoading(true);
    try {
      const data = await HomeAdminService.getAdminData();
      setCarousels(data.carousels);
      setStats(data.stats);
    } catch (err) {
      AlertService.error('Erro de Carregamento', err instanceof Error ? err.message : JSON.stringify(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (options.autoFetch) {
      fetchAdminData();
    }
  }, [fetchAdminData, options.autoFetch]);

  // --- Manipulação de Upload de Imagem ---
  const handleUploadImage = async (file: File): Promise<{ uploadId: string; uploadUrl: string } | null> => {
    setIsUploading(true);
    try {
      const response = await HomeAdminService.uploadImage(file);
      const uploadId = response.Id || response.id;
      const uploadUrl = response.Url || response.url;
      
      if (!uploadId) {
        throw new Error('ID de upload não retornado pelo servidor.');
      }

      return { uploadId, uploadUrl: uploadUrl || '' };
    } catch (err) {
      AlertService.error('Erro no upload', 'Não foi possível carregar a imagem. Tente novamente.');
      return null;
    } finally {
      setIsUploading(false);
    }
  };

  // --- Handlers de Gestão ---
  const handleDeleteCarousel = async (id: string) => {
    await AlertService.confirm({
      title: 'Remover Slide?',
      text: 'Esta ação não poderá ser desfeita no Dashboard.',
      onConfirm: async () => {
        try {
          await HomeAdminService.deleteCarousel(id);
          AlertService.success('Removido!', 'O slide foi excluído com sucesso.');
          fetchAdminData();
        } catch (err) {
          AlertService.error('Falha ao excluir', err instanceof Error ? err.message : JSON.stringify(err));
        }
      }
    });
  };

  const handleSaveCarousel = async (data: CarouselSaveDto, id?: string) => {
    setLoading(true);
    try {
      if (id) {
        await HomeAdminService.updateCarousel(id, data);
        AlertService.success('Atualizado', 'As alterações foram salvas.');
      } else {
        await HomeAdminService.createCarousel(data);
        AlertService.success('Criado', 'Novo slide adicionado ao carrossel.');
      }
      fetchAdminData();
    } catch (err) {
      AlertService.error('Erro ao salvar', err instanceof Error ? err.message : JSON.stringify(err));
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteStatCard = async (id: string) => {
    await AlertService.confirm({
      title: 'Remover Card?',
      text: 'Esta ação não poderá ser desfeita no Dashboard.',
      onConfirm: async () => {
        try {
          await HomeAdminService.deleteStatCard(id);
          AlertService.success('Removido!', 'O card de estatística foi excluído com sucesso.');
          fetchAdminData();
        } catch (err) {
          AlertService.error('Falha ao excluir', err instanceof Error ? err.message : JSON.stringify(err));
        }
      }
    });
  };

  const handleSaveStatCard = async (data: StatCardSaveDto, id?: string) => {
    setLoading(true);
    try {
      if (id) {
        await HomeAdminService.updateStatCard(id, data);
        AlertService.success('Atualizado', 'As alterações foram salvas.');
      } else {
        await HomeAdminService.createStatCard(data);
        AlertService.success('Criado', 'Novo card adicionado às estatísticas.');
      }
      fetchAdminData();
    } catch (err) {
      AlertService.error('Erro ao salvar', err instanceof Error ? err.message : JSON.stringify(err));
    } finally {
      setLoading(false);
    }
  };

  return {
    carousels,
    stats,
    loading,
    isUploading,
    refresh: fetchAdminData,
    uploadImage: handleUploadImage,
    deleteCarousel: handleDeleteCarousel,
    saveCarousel: handleSaveCarousel,
    deleteStatCard: handleDeleteStatCard,
    saveStatCard: handleSaveStatCard
  };
}