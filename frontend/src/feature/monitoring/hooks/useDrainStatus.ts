import { useState, useEffect, useCallback, useRef } from 'react';
import { AlertService } from '@/core/alert/AlertService';
import { isMockDataSourceEnabled } from '@/core/http/environment';
import { MonitoringService } from '../services/MonitoringService';
import { withMockLatency } from '@/core/mock/mockDelay';
import {
  MONITORING_MOCK_INITIAL_DELAY_MS,
  MONITORING_MOCK_UPDATE_INTERVAL_MS,
  createMockDrainStatusSnapshot,
  getNextMockDrainStatus,
} from '../mocks/monitoringMocks';
import type { DrainStatus } from '../types';

export const useDrainStatus = (bueiroId: string) => {
  const [data, setData] = useState<DrainStatus | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const intervalRef = useRef<ReturnType<typeof globalThis.setInterval> | null>(null);
  const unsubscribeRef = useRef<(() => void) | null>(null);
  const currentFrameRef = useRef(0);
  const isMountedRef = useRef(true);

  const clearRealtimeConnections = useCallback(() => {
    if (intervalRef.current !== null) {
      globalThis.clearInterval(intervalRef.current);
      intervalRef.current = null;
    }

    if (unsubscribeRef.current !== null) {
      unsubscribeRef.current();
      unsubscribeRef.current = null;
    }
  }, []);

  const startRealtimeUpdates = useCallback(() => {
    clearRealtimeConnections();

    intervalRef.current = globalThis.setInterval(() => {
      const [nextSnapshot, nextFrameIndex] = getNextMockDrainStatus(bueiroId, currentFrameRef.current);

      currentFrameRef.current = nextFrameIndex;
      setData(nextSnapshot);
    }, MONITORING_MOCK_UPDATE_INTERVAL_MS);
  }, [bueiroId, clearRealtimeConnections]);

  const startBackendUpdates = useCallback(() => {
    clearRealtimeConnections();

    unsubscribeRef.current = MonitoringService.subscribeToUpdates((payload) => {
      if (payload.id_bueiro === bueiroId) {
        setData(payload);
      }
    });
  }, [bueiroId, clearRealtimeConnections]);

  // 1. Lógica de Busca Inicial
  const fetchInitial = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      clearRealtimeConnections();
      currentFrameRef.current = 0;

      if (isMockDataSourceEnabled()) {
        await withMockLatency(() => undefined, MONITORING_MOCK_INITIAL_DELAY_MS);

        if (!isMountedRef.current) {
          return;
        }

        setData(createMockDrainStatusSnapshot(bueiroId, currentFrameRef.current));
        startRealtimeUpdates();
      } else {
        const response = await MonitoringService.getInitialStatus(bueiroId);

        if (!isMountedRef.current) {
          return;
        }

        setData(response);
        startBackendUpdates();
      }

      setError(null);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Erro ao carregar dados iniciais';
      setError(msg);
      AlertService.error('Erro', msg);
    } finally {
      if (isMountedRef.current) {
        setLoading(false);
      }
    }
  }, [bueiroId, clearRealtimeConnections, startBackendUpdates, startRealtimeUpdates]);

  useEffect(() => {
    isMountedRef.current = true;
    void fetchInitial();

    return () => {
      isMountedRef.current = false;
      clearRealtimeConnections();
    };
  }, [clearRealtimeConnections, fetchInitial]);

  return { data, loading, error, refetch: fetchInitial };
};