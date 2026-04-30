import { useState, useEffect, useCallback, useRef } from 'react';
import { AlertService } from '@/core/alert/AlertService';
import { MonitoringService } from '../services/MonitoringService';
import type { DrainLookup } from '../types';

export const useDrainsList = () => {
  const [data, setData] = useState<DrainLookup[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const isMountedRef = useRef(true);

  const fetchDrains = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await MonitoringService.getAvailableDrains();

      if (!isMountedRef.current) {
        return;
      }

      setData(response);
      setError(null);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Erro ao carregar lista de bueiros';
      if (isMountedRef.current) {
        setError(msg);
      }
      AlertService.error('Erro', msg);
    } finally {
      if (isMountedRef.current) {
        setLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    isMountedRef.current = true;
    void fetchDrains();

    return () => {
      isMountedRef.current = false;
    };
  }, [fetchDrains]);

  return { data, loading, error, refetch: fetchDrains };
};
