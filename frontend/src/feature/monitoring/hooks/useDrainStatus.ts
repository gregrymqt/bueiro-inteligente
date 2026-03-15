import { useState, useEffect, useCallback } from 'react';
import { MonitoringService } from '../services/MonitoringService';
import type { DrainStatusDTO } from '../types';

export const useDrainStatus = (bueiroId: string, pollIntervalMs: number = 5000) => {
  const [data, setData] = useState<DrainStatusDTO | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStatus = useCallback(async () => {
    try {
      // O Hook agora consome o Service de forma isolada
      const response = await MonitoringService.getDrainStatus(bueiroId);
      setData(response);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao conectar com o sensor.');
    } finally {
      setLoading(false);
    }
  }, [bueiroId]);

  useEffect(() => {
    fetchStatus();

    if (pollIntervalMs > 0) {
      const intervalId = setInterval(fetchStatus, pollIntervalMs);
      return () => clearInterval(intervalId);
    }
  }, [fetchStatus, pollIntervalMs]);

  return { data, loading, error, refetch: fetchStatus };
};