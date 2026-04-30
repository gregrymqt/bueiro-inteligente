import { useState, useEffect, useCallback, useRef } from 'react';
import { AlertService } from '@/core/alert/AlertService';
import type { Drain } from '@/feature/drain/types';
import { DrainService } from '@/feature/drain/services/DrainService';

export const useDrainsList = () => {
  // Alterado de DrainLookup[] para Drain[]
  const [data, setData] = useState<Drain[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const isMountedRef = useRef(true);

  const fetchDrains = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      // Passamos 'false' para não usar mock (ou controle conforme sua env)
      const response = await DrainService.getDrains(false);

      if (!isMountedRef.current) {
        return;
      }

      // Ordena alfabeticamente para o seletor (Dashboard) ficar organizado
      const sortedDrains = [...response].sort((left, right) => 
        left.name.localeCompare(right.name, 'pt-BR', { sensitivity: 'base' })
      );

      setData(sortedDrains);
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