import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@/core/http/ApiClient';
import { DrainService } from '../services/DrainService';

vi.mock('@/core/http/ApiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}));

const mockedApiClient = vi.mocked(apiClient);

describe('DrainService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('lista os bueiros usando o endpoint administrativo', async () => {
    const drains = [
      {
        id: '1',
        name: 'Bueiro Central',
        address: 'Rua A',
        latitude: -23.5,
        longitude: -46.6,
        hardware_id: 'HW-001',
        is_active: true,
      },
    ];

    mockedApiClient.get.mockResolvedValueOnce(drains);

    await expect(DrainService.getDrains()).resolves.toEqual(drains);
    expect(mockedApiClient.get).toHaveBeenCalledWith('/admin/drains');
  });

  it('cria um bueiro com o payload esperado', async () => {
    const payload = {
      name: 'Bueiro Norte',
      address: 'Rua B',
      latitude: -23.4,
      longitude: -46.5,
      hardware_id: 'HW-002',
      is_active: true,
    };

    mockedApiClient.post.mockResolvedValueOnce({ id: '2', ...payload });

    await expect(DrainService.createDrain(payload)).resolves.toEqual({ id: '2', ...payload });
    expect(mockedApiClient.post).toHaveBeenCalledWith('/admin/drains', payload);
  });

  it('atualiza um bueiro por id', async () => {
    const payload = {
      name: 'Bueiro Atualizado',
      hardware_id: 'HW-999',
    };

    mockedApiClient.patch.mockResolvedValueOnce({
      id: '3',
      name: 'Bueiro Atualizado',
      address: 'Rua C',
      latitude: -23.3,
      longitude: -46.4,
      hardware_id: 'HW-999',
      is_active: false,
    });

    await DrainService.updateDrain('3', payload);
    expect(mockedApiClient.patch).toHaveBeenCalledWith('/admin/drains/3', payload);
  });

  it('remove um bueiro por id', async () => {
    mockedApiClient.delete.mockResolvedValueOnce({});

    await DrainService.deleteDrain('4');
    expect(mockedApiClient.delete).toHaveBeenCalledWith('/admin/drains/4');
  });
});