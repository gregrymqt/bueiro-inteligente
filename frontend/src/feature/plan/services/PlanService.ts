// feature/plan/services/PlanService.ts[cite: 41]
import { apiClient } from "@/core/http/ApiClient";
import type { PricingPlan, PricingPlanCreatePayload, PricingPlanUpdatePayload } from "../types";

export class PlanService {
  private static readonly BASE_API = "/api/v1/plans";

  /**
   * [PÚBLICO] Busca planos ativos para a Home (Pricing Section)
   */
  public static async getActivePlans(): Promise<PricingPlan[]> {
    return apiClient.get<PricingPlan[]>(`${this.BASE_API}/active`);
  }

  /**
   * [ADMIN] Busca todos os planos (incluindo inativos)
   */
  public static async getAllPlans(): Promise<PricingPlan[]> {
    return apiClient.get<PricingPlan[]>(`${this.BASE_API}/all`);
  }

  /**
   * Busca os detalhes de um único plano pelo seu ID
   */
  public static async getPlanById(id: string): Promise<PricingPlan> {
    return apiClient.get<PricingPlan>(`${this.BASE_API}/${id}`);
  }

  /**
   * [ADMIN] Cria um novo plano integrado ao Mercado Pago
   */
  public static async createPlan(payload: PricingPlanCreatePayload): Promise<PricingPlan> {
    return apiClient.post<PricingPlan>(this.BASE_API, payload);
  }

  /**
   * [ADMIN] Atualiza informações visuais e de preço do plano
   */
  public static async updatePlan(id: string, payload: PricingPlanUpdatePayload): Promise<PricingPlan> {
    return apiClient.put<PricingPlan>(`${this.BASE_API}/${id}`, payload);
  }
  
  /**
   * [ADMIN] Alterna o status do plano (Inativação Lógica)
   */
  public static async updatePlanStatus(id: string, status: 'active' | 'inactive'): Promise<void> {
    // Enviamos para o endpoint PATCH conforme definido na Controller[cite: 40]
    return apiClient.patch(`${this.BASE_API}/${id}/status`, { status });
  }
}