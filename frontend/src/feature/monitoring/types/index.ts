export interface DrainStatusDTO {
  id_bueiro: string;
  distancia_cm: number;
  nivel_obstrucao: number;
  status: 'normal' | 'alerta' | 'critico' | 'Normal' | 'Alerta' | 'Crítico';
  latitude: number | null;
  longitude: number | null;
  ultima_atualizacao: string;
}