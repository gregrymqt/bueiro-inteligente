import { useCallback, useEffect, useState } from 'react';
import { AlertService } from '@/core/alert/AlertService';
import { DrainService } from '../services/DrainService';
import type { Drain, DrainCreatePayload, DrainUpdatePayload } from '../types';

const USE_DRAIN_ADMIN_MOCK = false;

const sortDrains = (drains: Drain[]): Drain[] => {
  return [...drains].sort((left, right) => left.name.localeCompare(right.name, 'pt-BR', { sensitivity: 'base' }));
};

const getErrorMessage = (error: unknown, fallbackMessage: string): string => {
  return error instanceof Error ? error.message : fallbackMessage;
};

export function useDrainAdmin(useMockParam?: boolean) {
  const useMock = useMockParam ?? USE_DRAIN_ADMIN_MOCK;
  const [drains, setDrains] = useState<Drain[]>([]);
  const [loading, setLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const fetchDrains = useCallback(async () => {
    setLoading(true);

    try {
      const items = await DrainService.getDrains(useMock);
      setDrains(sortDrains(items));
    } catch (error: unknown) {
      AlertService.error('Erro', getErrorMessage(error, 'Falha ao carregar os bueiros.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchDrains();
  }, [fetchDrains]);

  const createDrain = async (payload: DrainCreatePayload): Promise<boolean> => {
    setIsSaving(true);

    try {
      const createdDrain = await DrainService.createDrain(payload, useMock);
      setDrains((previousDrains) => sortDrains([...previousDrains, createdDrain]));
      AlertService.success('Bueiro criado com sucesso!');
      return true;
    } catch (error: unknown) {
      AlertService.error('Erro', getErrorMessage(error, 'Não foi possível criar o bueiro.'));
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const updateDrain = async (id: string, payload: DrainUpdatePayload): Promise<boolean> => {
    setIsSaving(true);

    try {
      const updatedDrain = await DrainService.updateDrain(id, payload, useMock);
      setDrains((previousDrains) =>
        sortDrains(previousDrains.map((item) => (item.id === id ? updatedDrain : item)))
      );
      AlertService.success('Bueiro atualizado com sucesso!');
      return true;
    } catch (error: unknown) {
      AlertService.error('Erro', getErrorMessage(error, 'Não foi possível atualizar o bueiro.'));
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const deleteDrain = async (id: string): Promise<boolean> => {
    setIsSaving(true);

    try {
      await DrainService.deleteDrain(id, useMock);
      setDrains((previousDrains) => previousDrains.filter((item) => item.id !== id));
      AlertService.success('Bueiro excluído com sucesso!');
      return true;
    } catch (error: unknown) {
      AlertService.error('Erro', getErrorMessage(error, 'Não foi possível excluir o bueiro.'));
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  return {
    drains,
    loading,
    isSaving,
    refreshDrains: fetchDrains,
    createDrain,
    updateDrain,
    deleteDrain,
  };
}