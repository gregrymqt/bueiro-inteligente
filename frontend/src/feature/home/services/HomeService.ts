import { apiClient } from '@/core/http/ApiClient';
import type { CarouselContent, CarouselSection } from '../types';

export class HomeService {
  /**
   * Busca itens do carousel baseados na seção (Hero, Alerts, etc)
   */
  public static async getCarouselItems(section: CarouselSection): Promise<CarouselContent[]> {
    // A rota no FastAPI deve esperar o parâmetro de seção
    return apiClient.get<CarouselContent[]>(`/home/carousel/${section}`);
  }

  /**
   * Método para atualizar um item (Útil para o seu futuro painel Admin)
   */
  public static async updateCarouselItem(id: string, data: Partial<CarouselContent>): Promise<void> {
    return apiClient.patch(`/home/carousel/items/${id}`, data);
  }
}