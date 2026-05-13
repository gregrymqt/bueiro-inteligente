// feature/home/services/HomeAdminService.ts
import { apiClient } from "@/core/http/ApiClient";
import type { 
  HomeAdminResponse, 
  CarouselSaveDto, 
  CarouselResponse, 
  StatCardSaveDto, 
  StatCardResponse,
  UploadDto
} from "../types/homeAdmin.types";

export class HomeAdminService {
  private static readonly ADMIN_API = "/api/v1/homeadmin";
  private static readonly PUBLIC_API = "/api/v1/home";

  public static async getAdminData(): Promise<HomeAdminResponse> {
    return apiClient.get<HomeAdminResponse>(this.PUBLIC_API);
  }

  public static async uploadImage(file: File): Promise<UploadDto> {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.postFile<UploadDto>('/api/v1/uploads', formData);
  }

  // --- Operações de Carousel ---
  public static async createCarousel(data: CarouselSaveDto): Promise<CarouselResponse> {
    return apiClient.post<CarouselResponse>(`${this.ADMIN_API}/carousel`, data);
  }

  // ATUALIZADO: Utiliza apiClient.patch alinhado com o [HttpPatch] do C#
  public static async updateCarousel(id: string, data: Partial<CarouselSaveDto>): Promise<CarouselResponse> {
    return apiClient.patch<CarouselResponse>(`${this.ADMIN_API}/carousel/${id}`, data);
  }

  public static async deleteCarousel(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.ADMIN_API}/carousel/${id}`);
  }

  // --- Operações de StatCards ---
  public static async createStatCard(data: StatCardSaveDto): Promise<StatCardResponse> {
    return apiClient.post<StatCardResponse>(`${this.ADMIN_API}/stats`, data);
  }

  // ATUALIZADO: Utiliza apiClient.patch alinhado com o [HttpPatch] do C#
  public static async updateStatCard(id: string, data: Partial<StatCardSaveDto>): Promise<StatCardResponse> {
    return apiClient.patch<StatCardResponse>(`${this.ADMIN_API}/stats/${id}`, data);
  }

  public static async deleteStatCard(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.ADMIN_API}/stats/${id}`);
  }
}