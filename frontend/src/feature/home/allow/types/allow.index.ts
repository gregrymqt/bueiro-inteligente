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
  order: number;
  avatarUrl?: string;
}

export type CarouselSection = 'hero' | 'alerts' | 'stats';

export interface CarouselContent {
  id: string;
  title: string;
  subtitle?: string | null;
  desktop_image_url: string;
  mobile_image_url: string;
  action_url?: string | null;
  order: number;
  section: CarouselSection;
}

export interface CarouselCreatePayload {
  title: string;
  subtitle?: string | null;
  action_url?: string | null;
  order: number;
  section: CarouselSection;
}

export type CarouselUpdatePayload = Partial<CarouselCreatePayload>;

export type StatCardColor = 'success' | 'warning' | 'danger';

export interface StatCardContent {
  id: string;
  title: string;
  value: string;
  description: string;
  icon_name: string;
  color: StatCardColor;
  order: number;
}

export interface StatCardCreatePayload {
  title: string;
  value: string;
  description: string;
  icon_name: string;
  color: StatCardColor;
  order: number;
}

export type StatCardUpdatePayload = Partial<StatCardCreatePayload>;

// Resposta unificada que a Home precisará consumir
export interface LandingPageData {
  steps: HowItWorksStep[];
  plans: PricingPlan[];
  reviews: UserReview[];
}

export interface HomeDataResponse {
  carousels: CarouselContent[];
  stats: StatCardContent[];
  plans: PricingPlan[];
  reviews: UserReview[];
}