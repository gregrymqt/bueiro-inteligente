export interface PricingPlan {
  id: string;
  name: string;
  price: number; // Mapeado do decimal 'Price' do C#
  status: 'active' | 'inactive'; // C# agora envia minúsculo conforme o banco[cite: 39, 42]
  initPoint: string; // URL do Mercado Pago vinda do DTO[cite: 39, 42]
  features: string[]; // Lista de benefícios[cite: 42]
  isPopular: boolean; // Flag para UI de destaque[cite: 39, 42]
}

export interface PricingPlanCreatePayload {
  name: string;
  amount: number; // O backend C# espera 'Amount' no CreateRequest[cite: 39]
  features: string[];
  isPopular: boolean;
  frequency: number;
  frequencyType: string;
  backUrl?: string;
}

// Para atualização, usamos o DTO específico do C#[cite: 39, 42]
export interface PricingPlanUpdatePayload {
  name: string;
  amount: number;
  features: string[];
  isPopular: boolean;
}