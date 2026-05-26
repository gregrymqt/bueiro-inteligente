export interface Drain {
  id: string;
  name: string;
  address: string;
  latitude: number;
  longitude: number;
  hardware_id: string;
  is_active: boolean;
  status?: string;
  nivel_obstrucao?: number;
  distancia_cm?: number;
  ultima_atualizacao?: string;
}

export type DrainCreatePayload = Omit<Drain, 'id'>;

export type DrainUpdatePayload = Partial<DrainCreatePayload>;