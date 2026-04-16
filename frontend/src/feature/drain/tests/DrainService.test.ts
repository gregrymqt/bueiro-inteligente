import { beforeEach, describe, expect, it } from 'vitest';
import { resetMockDrains } from '../mocks/drainMocks';
import { DrainService } from '../services/DrainService';

const USE_DRAIN_MOCK = true;

describe('DrainService', () => {
  beforeEach(() => {
    resetMockDrains();
  });

  it('lista os bueiros a partir do mock local', async () => {
    const drains = await DrainService.getDrains(USE_DRAIN_MOCK);

    expect(drains).toHaveLength(4);
    expect(drains.map((drain) => drain.name)).toEqual([
      'Bueiro Central',
      'Bueiro Jardim Europa',
      'Bueiro Terminal Norte',
      'Bueiro Viaduto Sul',
    ]);
  });

  it('cria um bueiro e mantém o estado mockado atualizado', async () => {
    const payload = {
      name: 'Bueiro Nova Esperança',
      address: 'Rua das Flores, 99 - Centro',
      latitude: -23.5489,
      longitude: -46.6388,
      hardware_id: 'ESP32-NEW-05',
      is_active: true,
    };

    const createdDrain = await DrainService.createDrain(payload, USE_DRAIN_MOCK);

    expect(createdDrain).toMatchObject({
      id: expect.stringMatching(/^drain-/),
      ...payload,
    });
    await expect(DrainService.getDrainById(createdDrain.id, USE_DRAIN_MOCK)).resolves.toMatchObject(payload);
    await expect(DrainService.getDrains(USE_DRAIN_MOCK)).resolves.toHaveLength(5);
  });

  it('atualiza um bueiro por id', async () => {
    const updatedDrain = await DrainService.updateDrain('drain-03', {
      name: 'Bueiro Jardim Europa Atualizado',
      is_active: true,
    }, USE_DRAIN_MOCK);

    expect(updatedDrain).toMatchObject({
      id: 'drain-03',
      name: 'Bueiro Jardim Europa Atualizado',
      is_active: true,
    });
    await expect(DrainService.getDrainById('drain-03', USE_DRAIN_MOCK)).resolves.toMatchObject({
      name: 'Bueiro Jardim Europa Atualizado',
      is_active: true,
    });
  });

  it('remove um bueiro por id', async () => {
    await DrainService.deleteDrain('drain-04', USE_DRAIN_MOCK);

    await expect(DrainService.getDrainById('drain-04', USE_DRAIN_MOCK)).rejects.toThrow('não encontrado');
    await expect(DrainService.getDrains(USE_DRAIN_MOCK)).resolves.toHaveLength(3);
  });
});