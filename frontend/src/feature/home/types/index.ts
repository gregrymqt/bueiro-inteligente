import type { JSX } from "react";

export type CarouselSection = 'hero' | 'alerts' | 'stats';

export interface CarouselContent {
  id: string;
  title: string;
  subtitle?: string;
  imageUrl: string;
  actionUrl?: string; // Link para onde o slide redireciona
  order: number;
}

// Interface para facilitar o gerenciamento por seção no Dashboard
export interface HomeCarouselState {
  section: CarouselSection;
  items: CarouselContent[];
}

export interface StatCardContent {
  id: string;
  title: string;
  value: string; // Ex: "85%", "12 Criticos"
  description: string;
  icon: JSX.Element; // Ícone do Lucide-React
  color: 'success' | 'warning' | 'danger'; // Cor do Design Token
  order: number;
}