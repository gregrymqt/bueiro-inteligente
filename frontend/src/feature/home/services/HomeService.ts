import { apiClient } from '@/core/http/ApiClient';
import type { 
  HomeDataResponse, 
  CarouselContent, 
  CarouselCreatePayload,
  CarouselUpdatePayload,
  StatCardContent,
  StatCardCreatePayload,
  StatCardUpdatePayload
} from '../types';

export class HomeService {
  /**
   * [PUBLIC] Busca todos os dados da Home (Carousels e Stats).
   * Chama a rota pública otimizada por cache do backend.
   */
  public static async getHomeData(): Promise<HomeDataResponse> {
    return apiClient.get<HomeDataResponse>('/home');
  }

  // ==========================================
  // Operações Administrativas - Carousel
  // ==========================================

  /**
   * [ADMIN] Cria uma nova imagem/alerta para o Carousel da Home.
   */
  public static async createCarouselItem(data: CarouselCreatePayload): Promise<CarouselContent> {
    return apiClient.post<CarouselContent>('/admin/home/carousel', data);
  }

  /**
   * [ADMIN] Atualiza parcialmente um item do Carousel existente.
   */
  public static async updateCarouselItem(id: string, data: CarouselUpdatePayload): Promise<CarouselContent> {
    return apiClient.patch<CarouselContent>(`/admin/home/carousel/${id}`, data);
  }

  /**
   * [ADMIN] Deleta um item do Carousel da Home.
   */
  public static async deleteCarouselItem(id: string): Promise<void> {
    return apiClient.delete(`/admin/home/carousel/${id}`);
  }

  // ==========================================
  // Operações Administrativas - StatCards
  // ==========================================

  /**
   * [ADMIN] Cria um novo Card Estatístico para a Home.
   */
  public static async createStatCard(data: StatCardCreatePayload): Promise<StatCardContent> {
    return apiClient.post<StatCardContent>('/admin/home/stats', data);
  }

  /**
   * [ADMIN] Atualiza parcialmente um Card Estatístico existente.
   */
  public static async updateStatCard(id: string, data: StatCardUpdatePayload): Promise<StatCardContent> {
    return apiClient.patch<StatCardContent>(`/admin/home/stats/${id}`, data);
  }

  /**
   * [ADMIN] Deleta um Card Estatístico da Home.
   */
  public static async deleteStatCard(id: string): Promise<void> {
    return apiClient.delete(`/admin/home/stats/${id}`);
  }
}