import { withMockLatency } from '@/core/mock/mockDelay';
import type { LoginRequestDTO, LoginResponseDTO, RegisterRequestDTO, UserDTO } from '../types';

export const mockAuthToken = 'eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzdWIiOiJtYXJpYS5zaWx2YUBidWVpcm9pbnRlbGlnZW50ZS5jb20iLCJyb2xlIjoiYWRtaW4iLCJleHAiOjE4OTM0NTYwMDB9.mock-signature';

export const mockAuthenticatedUser: UserDTO = {
  email: 'maria.silva@bueirointeligente.com',
  full_name: 'Maria Silva',
  roles: ['admin', 'manutencao'],
};

export const mockDemoCredentials: LoginRequestDTO = {
  email: 'demo@bueirointeligente.com',
  password: 'Demo#1234',
};

const createMockRegisteredUser = (payload: RegisterRequestDTO): UserDTO => ({
  email: payload.email.trim(),
  full_name: payload.full_name?.trim() || 'Usuário Demo',
  roles: ['cidadao'],
});

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export const loginWithMockCredentials = async (credentials: LoginRequestDTO): Promise<LoginResponseDTO> =>
  withMockLatency(() => ({
    access_token: mockAuthToken,
    token_type: 'Bearer',
  }), 240);

export const registerWithMockData = async (payload: RegisterRequestDTO): Promise<UserDTO> =>
  withMockLatency(() => createMockRegisteredUser(payload), 280);

export const getMockAuthenticatedUser = async (): Promise<UserDTO> =>
  withMockLatency(() => ({
    ...mockAuthenticatedUser,
    roles: [...mockAuthenticatedUser.roles],
  }), 180);

export const logoutMockSession = async (): Promise<void> =>
  withMockLatency(() => undefined, 120);