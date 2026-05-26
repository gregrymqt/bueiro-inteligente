// feature/home/types/homeAdmin.types.ts

// Enums espelhados do Backend
export type CarouselSection = 'hero' | 'alerts' | 'stats';
export type StatCardColor = 'success' | 'warning' | 'danger';

// DTO de Retorno do Upload de Arquivos
export interface UploadDto {
  id: string;
  fileName: string;
  contentType: string;
  size: number;
  url: string;
  createdAt: string;
}

export interface UploadImagesDto {
  desktop: UploadDto;
  mobile: UploadDto;
}

// Respostas (GET)
export interface CarouselResponse {
  id: string;
  title: string;
  subtitle: string | null;
  desktop_image_url: string;
  mobile_image_url: string;
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
  desktop_upload_id: string;
  mobile_upload_id: string;
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