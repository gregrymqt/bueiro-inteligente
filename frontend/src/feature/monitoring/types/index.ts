export interface DrainLookup {
  id: string;
  nome: string;
}

export interface DrainStatus {
  id_bueiro: string;
  distancia_cm: number;
  nivel_obstrucao: number;
  status: string;
  latitude?: number;
  longitude?: number;
  ultima_atualizacao: string; // ISO Date do Pydantic
}