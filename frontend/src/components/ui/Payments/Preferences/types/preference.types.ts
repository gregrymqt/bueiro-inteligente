/**
 * Payload enviado para a PreferenceController[cite: 40, 41]
 */
export interface CreatePreferenceRequest {
  title: string;
  description: string;
  unitPrice: number;
  payerEmail: string;
  planId?: string;
}

/**
 * Resposta retornada pela PreferenceController[cite: 40]
 */
export interface PreferenceResponse {
  preferenceId: string;
  initPoint: string; // Pode ser usado como fallback se o Brick falhar[cite: 40]
  externalReference: string;
}