export interface PricingPlan {
  id: string;
  name: string;
  price: number;
  features: string[];
  isPopular?: boolean;
}

// Futuramente, se houver payload de criação/edição no painel Admin, adicionamos aqui:
export type PricingPlanCreatePayload = Omit<PricingPlan, 'id'>;
export type PricingPlanUpdatePayload = Partial<PricingPlanCreatePayload>;