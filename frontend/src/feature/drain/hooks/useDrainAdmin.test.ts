import { act, cleanup, renderHook, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import type { Drain, DrainCreatePayload, DrainUpdatePayload } from '../types';
import { useDrainAdmin } from './useDrainAdmin';

const drainServiceMocks = vi.hoisted(() => {
  const getDrains = vi.fn<(useMock: boolean) => Promise<Drain[]>>();
  const createDrain = vi.fn<(payload: DrainCreatePayload, useMock: boolean) => Promise<Drain>>();
  const updateDrain = vi.fn<(id: string, payload: DrainUpdatePayload, useMock: boolean) => Promise<Drain>>();
  const deleteDrain = vi.fn<(id: string, useMock: boolean) => Promise<void>>();

  return {
    getDrains,
    createDrain,
    updateDrain,
    deleteDrain,
  };
});

const alertServiceMocks = vi.hoisted(() => {
  const success = vi.fn<(title: string, text?: string) => void>();
  const error = vi.fn<(title: string, text?: string) => void>();

  return {
    success,
    error,
  };
});

vi.mock('../services/DrainService', () => ({
  DrainService: drainServiceMocks,
}));

vi.mock('@/core/alert/AlertService', () => ({
  AlertService: alertServiceMocks,
}));

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

const buildDrain = (overrides: Partial<Drain> = {}): Drain => ({
  id: overrides.id ?? '1',
  name: overrides.name ?? 'Bueiro Central',
  address: overrides.address ?? 'Rua Central, 10',
  latitude: overrides.latitude ?? -23.55,
  longitude: overrides.longitude ?? -46.63,
  hardware_id: overrides.hardware_id ?? 'HW-001',
  is_active: overrides.is_active ?? true,
});

const createDeferred = <T,>() => {
  let resolvePromise: (value: T) => void = () => undefined;
  const promise = new Promise<T>((resolve) => {
    resolvePromise = resolve;
  });

  return {
    promise,
    resolvePromise,
  };
};

describe('useDrainAdmin', () => {
  it('carrega a lista no mount e alterna loading', async () => {
    const deferred = createDeferred<Drain[]>();
    const drains = [buildDrain({ id: '1', name: 'Bueiro Centro' })];

    drainServiceMocks.getDrains.mockReturnValueOnce(deferred.promise);

    const { result } = renderHook(() => useDrainAdmin());

    await waitFor(() => {
      expect(result.current.loading).toBe(true);
    });

    await act(async () => {
      deferred.resolvePromise(drains);
      await deferred.promise;
    });

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
      expect(result.current.drains).toEqual(drains);
    });

    expect(drainServiceMocks.getDrains).toHaveBeenCalledWith(false);
    expect(alertServiceMocks.error).not.toHaveBeenCalled();
  });

  it('dispara sucesso ao criar bueiro', async () => {
    const initialDrains: Drain[] = [];
    const payload: DrainCreatePayload = {
      name: 'Bueiro Novo',
      address: 'Rua Nova, 11',
      latitude: -23.551,
      longitude: -46.631,
      hardware_id: 'HW-200',
      is_active: true,
    };
    const createdDrain = buildDrain({
      id: '2',
      name: payload.name,
      address: payload.address,
      latitude: payload.latitude,
      longitude: payload.longitude,
      hardware_id: payload.hardware_id,
      is_active: payload.is_active,
    });

    drainServiceMocks.getDrains.mockResolvedValueOnce(initialDrains);
    drainServiceMocks.createDrain.mockResolvedValueOnce(createdDrain);

    const { result } = renderHook(() => useDrainAdmin());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    let operationResult = false;

    await act(async () => {
      operationResult = await result.current.createDrain(payload);
    });

    expect(operationResult).toBe(true);
    expect(drainServiceMocks.createDrain).toHaveBeenCalledWith(payload, false);
    expect(alertServiceMocks.success).toHaveBeenCalledWith('Bueiro criado com sucesso!');
    expect(result.current.drains).toEqual([createdDrain]);
  });

  it('dispara erro ao falhar atualizar bueiro', async () => {
    const existingDrain = buildDrain({ id: '3' });
    const payload: DrainUpdatePayload = {
      name: 'Bueiro Atualizado',
    };

    drainServiceMocks.getDrains.mockResolvedValueOnce([existingDrain]);
    drainServiceMocks.updateDrain.mockRejectedValueOnce(new Error('Falha ao atualizar bueiro'));

    const { result } = renderHook(() => useDrainAdmin());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    let operationResult = true;

    await act(async () => {
      operationResult = await result.current.updateDrain(existingDrain.id, payload);
    });

    expect(operationResult).toBe(false);
    expect(drainServiceMocks.updateDrain).toHaveBeenCalledWith(existingDrain.id, payload, false);
    expect(alertServiceMocks.error).toHaveBeenCalledWith('Erro', 'Falha ao atualizar bueiro');
    expect(result.current.drains).toEqual([existingDrain]);
  });

  it('remove bueiro e emite sucesso ao excluir', async () => {
    const existingDrain = buildDrain({ id: '4', name: 'Bueiro Zona Sul' });

    drainServiceMocks.getDrains.mockResolvedValueOnce([existingDrain]);
    drainServiceMocks.deleteDrain.mockResolvedValueOnce();

    const { result } = renderHook(() => useDrainAdmin());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    let operationResult = false;

    await act(async () => {
      operationResult = await result.current.deleteDrain(existingDrain.id);
    });

    expect(operationResult).toBe(true);
    expect(drainServiceMocks.deleteDrain).toHaveBeenCalledWith(existingDrain.id, false);
    expect(alertServiceMocks.success).toHaveBeenCalledWith('Bueiro excluído com sucesso!');
    expect(result.current.drains).toEqual([]);
  });
});