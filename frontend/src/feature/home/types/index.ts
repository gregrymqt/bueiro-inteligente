
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

// Resposta unificada que o seu GET /home deve retornar
export interface HomeDataResponse {
  carousels: CarouselContent[];
  stats: StatCardContent[];
}