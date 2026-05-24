export interface DrainStatus {
  // Metadados adicionais
  id?: string;
  name?: string;
  address?: string;

  // Propriedades SignalR (camelCase)
  idBueiro: string;
  distanciaCm: number;
  nivelObstrucao: number;
  status: string;
  latitude?: number;
  longitude?: number;
  ultimaAtualizacao: string;

  // Fallbacks para compatibilidade REST API (snake_case)
  id_bueiro?: string;
  distancia_cm?: number;
  nivel_obstrucao?: number;
  ultima_atualizacao?: string;
}