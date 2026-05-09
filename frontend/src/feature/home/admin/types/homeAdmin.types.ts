// Enums espelhados do Backend
export type CarouselSection = 'hero' | 'alerts' | 'stats';
export type StatCardColor = 'success' | 'warning' | 'danger';

// Respostas (GET)
export interface CarouselResponse {
  id: string;
  title: string;
  subtitle: string | null;
  image_url: string;
  action_url: string | null;
  order: number;
  section: CarouselSection;
}

export interface StatCardResponse {
  id: string;
  title: string;
  value: string;
  description: string;
  icon_name: string;
  color: StatCardColor;
  order: number;
}

export interface HomeAdminResponse {
  carousels: CarouselResponse[];
  stats: StatCardResponse[];
}

// Payloads para Criar/Atualizar
export interface CarouselSaveDto {
  title: string;
  subtitle?: string | null;
  upload_id: string; // Guid retornado pelo seu serviço de upload
  action_url?: string | null;
  order: number;
  section: CarouselSection;
}

export interface StatCardSaveDto {
  title: string;
  value: string;
  description: string;
  icon_name: string;
  color: StatCardColor;
  order: number;
}