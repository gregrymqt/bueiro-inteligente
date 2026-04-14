import type { DrainStatus } from '../types';

export const MONITORING_MOCK_INITIAL_DELAY_MS = 360;
export const MONITORING_MOCK_UPDATE_INTERVAL_MS = 2400;

export interface RowsInsight {
  label: string;
  value: string;
  detail: string;
  tone: 'success' | 'warning' | 'danger';
}

export interface RowsTrendPoint {
  label: string;
  value: number;
}

const monitoringFrames: Array<Pick<DrainStatus, 'distancia_cm' | 'nivel_obstrucao' | 'status'>> = [
  {
    distancia_cm: 118,
    nivel_obstrucao: 8,
    status: 'NORMAL',
  },
  {
    distancia_cm: 96,
    nivel_obstrucao: 19,
    status: 'NORMAL',
  },
  {
    distancia_cm: 73,
    nivel_obstrucao: 41,
    status: 'ALERTA',
  },
  {
    distancia_cm: 52,
    nivel_obstrucao: 63,
    status: 'CRITICO',
  },
  {
    distancia_cm: 64,
    nivel_obstrucao: 49,
    status: 'ALERTA',
  },
];

export const mockRowsInsights: RowsInsight[] = [
  {
    label: 'Bueiros monitorados',
    value: '28',
    detail: '19 ativos com telemetria viva nesta apresentação.',
    tone: 'success',
  },
  {
    label: 'Alertas na última hora',
    value: '4',
    detail: '2 exigem observação preventiva da equipe.',
    tone: 'warning',
  },
  {
    label: 'Ocorrências críticas',
    value: '1',
    detail: 'Equipes já receberam o disparo operacional.',
    tone: 'danger',
  },
];

export const mockRowsTrendPoints: RowsTrendPoint[] = [
  { label: '08h', value: 18 },
  { label: '10h', value: 22 },
  { label: '12h', value: 33 },
  { label: '14h', value: 45 },
  { label: '16h', value: 61 },
  { label: '18h', value: 52 },
];

const getFrame = (frameIndex: number): Pick<DrainStatus, 'distancia_cm' | 'nivel_obstrucao' | 'status'> =>
  monitoringFrames[frameIndex % monitoringFrames.length];

export const createMockDrainStatusSnapshot = (bueiroId: string, frameIndex: number = 0): DrainStatus => {
  const frame = getFrame(frameIndex);

  return {
    id_bueiro: bueiroId,
    distancia_cm: frame.distancia_cm,
    nivel_obstrucao: frame.nivel_obstrucao,
    status: frame.status,
    latitude: -23.55052,
    longitude: -46.633308,
    ultima_atualizacao: new Date().toISOString(),
  };
};

export const getNextMockDrainStatus = (bueiroId: string, currentFrameIndex: number): [DrainStatus, number] => {
  const nextFrameIndex = (currentFrameIndex + 1) % monitoringFrames.length;

  return [createMockDrainStatusSnapshot(bueiroId, nextFrameIndex), nextFrameIndex];
};