import { useState, useEffect, useCallback } from 'react';
import { MonitoringService } from '../services/MonitoringService';
import type { DrainStatus } from '../types';

export const useDrainStatus = (bueiroId: string) => {
  const [data, setData] = useState<DrainStatus | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // 1. Lógica de Busca Inicial
  const fetchInitial = useCallback(async () => {
    try {
      setLoading(true);
      const response = await MonitoringService.getInitialStatus(bueiroId);
      setData(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar dados iniciais');
    } finally {
      setLoading(false);
    }
  }, [bueiroId]);

  useEffect(() => {
    fetchInitial();

    // 2. Inscrição no Real-time via Service
    const unsubscribe = MonitoringService.subscribeToUpdates((payload) => {
      // Regra de Negócio: Só atualiza se o evento for o correto e o ID bater
      if (payload.evento_tipo === "BUEIRO_STATUS_MUDOU" && payload.dados.id_bueiro === bueiroId) {
        setData(payload.dados);
      }
    });

    return () => unsubscribe(); // Limpa o listener ao desmontar
  }, [bueiroId, fetchInitial]);

  return { data, loading, error, refetch: fetchInitial };
};