import { withMockLatency } from '@/core/mock/mockDelay';
import type { Drain, DrainCreatePayload, DrainUpdatePayload } from '../types';

const mockDrainsSeed: Drain[] = [
  {
    id: 'drain-01',
    name: 'Bueiro Central',
    address: 'Av. Paulista, 1000 - Bela Vista',
    latitude: -23.561684,
    longitude: -46.655981,
    hardware_id: 'ESP32-CENTRAL-01',
    is_active: true,
  },
  {
    id: 'drain-02',
    name: 'Bueiro Terminal Norte',
    address: 'Av. Cruzeiro do Sul, 3200 - Santana',
    latitude: -23.500553,
    longitude: -46.624627,
    hardware_id: 'ESP32-NORTE-02',
    is_active: true,
  },
  {
    id: 'drain-03',
    name: 'Bueiro Jardim Europa',
    address: 'Rua Alemanha, 145 - Jardim Europa',
    latitude: -23.573676,
    longitude: -46.689625,
    hardware_id: 'ESP32-JE-03',
    is_active: false,
  },
  {
    id: 'drain-04',
    name: 'Bueiro Viaduto Sul',
    address: 'Av. do Estado, 5600 - Ipiranga',
    latitude: -23.589325,
    longitude: -46.607986,
    hardware_id: 'ESP32-SUL-04',
    is_active: true,
  },
];

const cloneDrain = (drain: Drain): Drain => ({
  ...drain,
});

const sortDrainsByName = (drains: Drain[]): Drain[] =>
  [...drains].sort((left, right) => left.name.localeCompare(right.name, 'pt-BR', { sensitivity: 'base' }));

let drainSequence = mockDrainsSeed.length + 1;
let drainsStore: Drain[] = mockDrainsSeed.map(cloneDrain);

const createDrainId = (): string => `drain-${String(drainSequence++).padStart(2, '0')}`;

const replaceStore = (nextDrains: Drain[]): void => {
  drainsStore = nextDrains.map(cloneDrain);
};

export const resetMockDrains = (): void => {
  drainSequence = mockDrainsSeed.length + 1;
  replaceStore(mockDrainsSeed);
};

export const getMockDrains = async (): Promise<Drain[]> =>
  withMockLatency(() => sortDrainsByName(drainsStore).map(cloneDrain), 240);

export const getMockDrainById = async (id: string): Promise<Drain> =>
  withMockLatency(() => {
    const drain = drainsStore.find((item) => item.id === id);

    if (!drain) {
      throw new Error(`Bueiro ${id} não encontrado.`);
    }

    return cloneDrain(drain);
  }, 220);

export const createMockDrain = async (payload: DrainCreatePayload): Promise<Drain> =>
  withMockLatency(() => {
    const createdDrain: Drain = {
      id: createDrainId(),
      ...payload,
    };

    replaceStore([...drainsStore, createdDrain]);

    return cloneDrain(createdDrain);
  }, 280);

export const updateMockDrain = async (id: string, payload: DrainUpdatePayload): Promise<Drain> =>
  withMockLatency(() => {
    const currentDrain = drainsStore.find((item) => item.id === id);

    if (!currentDrain) {
      throw new Error(`Bueiro ${id} não encontrado.`);
    }

    const updatedDrain: Drain = {
      ...currentDrain,
      ...payload,
    };

    replaceStore(drainsStore.map((item) => (item.id === id ? updatedDrain : item)));

    return cloneDrain(updatedDrain);
  }, 280);

export const deleteMockDrain = async (id: string): Promise<void> =>
  withMockLatency(() => {
    const existingDrain = drainsStore.find((item) => item.id === id);

    if (!existingDrain) {
      throw new Error(`Bueiro ${id} não encontrado.`);
    }

    replaceStore(drainsStore.filter((item) => item.id !== id));
  }, 220);