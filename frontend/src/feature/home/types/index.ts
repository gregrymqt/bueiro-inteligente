
export type CarouselSection = 'hero' | 'alerts' | 'stats';

export interface CarouselContent {
  id: string;
  title: string;
  subtitle?: string;
  image_url: string;
  action_url?: string; 
  order: number;
  section: CarouselSection; 
}

export interface CarouselCreatePayload {
  title: string;
  subtitle?: string;
  upload_id?: string;
  action_url?: string;
  order: number;
  section: CarouselSection;
}

export type CarouselUpdatePayload = Partial<CarouselCreatePayload>;

export interface StatCardContent {
  id: string;
  title: string;
  value: string; 
  description: string;
  icon_name: string; 
  color: 'success' | 'warning' | 'danger'; 
  order: number;
}

export type StatCardCreatePayload = Omit<StatCardContent, 'id'>;
export type StatCardUpdatePayload = Partial<StatCardCreatePayload>;

export interface PricingPlan {
  id: string;
  name: string;
  price: string;
  description: string;
  features: string[];
  isPopular: boolean;
  order: number;
}

export interface UserReview {
  id: string;
  authorName: string;
  authorRole: string;
  content: string;
  rating: number;
  avatarUrl?: string;
  order: number;
}

// Resposta unificada do GET /home.
// O backend atual retorna `carousels` e `stats`; `plans` e `reviews`
// podem ser acrescentados em releases futuras ou normalizados na camada
// de service/hook quando ausentes.
export interface HomeDataResponse {
  carousels: CarouselContent[];
  stats: StatCardContent[];
  plans?: PricingPlan[];
  reviews?: UserReview[];
}

export interface HomeConfiguration {
  id?: string;
  theme: string;
  contact_email: string;
}
