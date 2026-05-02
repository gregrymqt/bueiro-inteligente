import { apiClient } from "@/core/http/ApiClient";
import type { PricingPlan } from "../types";

export class PlanService {
  private static readonly BASE_API = "/api/v1/plans";

  /**
   * [PUBLIC] Busca os planos de serviço disponíveis
   */
  public static async getPlans(): Promise<PricingPlan[]> {
    // Como você usa mock no backend (opcional), vamos chamar a rota.
    // O backend pode retornar um mock interno ou os planos reais.
    return apiClient.get<PricingPlan[]>(this.BASE_API);
  }
}