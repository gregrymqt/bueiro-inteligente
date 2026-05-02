// Importamos o tipo da feature de planos respeitando a separação de domínios

import type { PricingPlan } from "@/feature/plan/types";

// Tipagem para a Seção: Como Funciona
export interface HowItWorksStep {
  id: string;
  title: string;
  description: string;
  icon_name: string;
  order: number;
}

// Tipagem para a Seção: Avaliações (Reviews)
export interface UserReview {
  id: string;
  userName: string;
  role: string;
  comment: string;
  rating: number; // Ex: 1 a 5
  avatarUrl?: string;
}

// Resposta unificada que a Home precisará consumir
export interface LandingPageData {
  steps: HowItWorksStep[];
  plans: PricingPlan[];
  reviews: UserReview[];
}