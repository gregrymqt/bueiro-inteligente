import { apiClient } from "@/core/http/ApiClient";
import type { LandingPageData } from "../types";

// Note: Removemos os métodos de MOCK temporariamente para focar na arquitetura limpa,
// se você precisar dos mocks antigos para painel admin, você os manterá num serviço separado (ex: HomeAdminService).
export class HomeService {
  private static readonly BASE_API = "/api/v1/home";

  /**
   * [PUBLIC] Busca todos os dados dinâmicos da Landing Page (Exceto Planos, que têm domínio próprio)
   */
  public static async getLandingPageData(): Promise<Omit<LandingPageData, 'plans'>> {
    // A rota retorna os steps (como funciona) e as reviews (avaliações).
    // Nota: O backend precisará ser ajustado para retornar este contrato.
    return apiClient.get<Omit<LandingPageData, 'plans'>>(this.BASE_API);
  }
}