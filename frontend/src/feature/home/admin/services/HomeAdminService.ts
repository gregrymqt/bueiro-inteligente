import { apiClient } from "@/core/http/ApiClient";
import type { 
  HomeAdminResponse, 
  CarouselSaveDto, 
  CarouselResponse, 
  StatCardSaveDto, 
  StatCardResponse 
} from "../types/homeAdmin.types";

export class HomeAdminService {
  private static readonly BASE_API = "/api/v1/home";

  // Obter tudo (Dashboard)
  public static async getAdminData(): Promise<HomeAdminResponse> {
    return apiClient.get<HomeAdminResponse>(this.BASE_API);
  }

  // --- Operações de Carousel ---
  public static async createCarousel(data: CarouselSaveDto): Promise<CarouselResponse> {
    return apiClient.post<CarouselResponse>(`${this.BASE_API}/carousel`, data);
  }

  public static async updateCarousel(id: string, data: Partial<CarouselSaveDto>): Promise<CarouselResponse> {
    return apiClient.put<CarouselResponse>(`${this.BASE_API}/carousel/${id}`, data);
  }

  public static async deleteCarousel(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.BASE_API}/carousel/${id}`);
  }

  // --- Operações de StatCards ---
  public static async createStatCard(data: StatCardSaveDto): Promise<StatCardResponse> {
    return apiClient.post<StatCardResponse>(`${this.BASE_API}/stats`, data);
  }

  public static async updateStatCard(id: string, data: Partial<StatCardSaveDto>): Promise<StatCardResponse> {
    return apiClient.put<StatCardResponse>(`${this.BASE_API}/stats/${id}`, data);
  }

  public static async deleteStatCard(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.BASE_API}/stats/${id}`);
  }
}