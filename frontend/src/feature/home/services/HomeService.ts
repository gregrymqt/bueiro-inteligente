import { apiClient } from "@/core/http/ApiClient";
import type {
  HomeDataResponse,
  CarouselContent,
  CarouselCreatePayload,
  CarouselUpdatePayload,
  StatCardContent,
  StatCardCreatePayload,
  StatCardUpdatePayload,
} from "../types";
import {
  createMockCarouselItem,
  createMockStatCard,
  deleteMockCarouselItem,
  deleteMockStatCard,
  getMockHomeData,
  updateMockCarouselItem,
  updateMockStatCard,
} from "../mocks/homeMocks";

export class HomeService {
  private static readonly BASE_API_ADMIN = "/api/v1/homeadmin";
  private static readonly BASE_API = "/api/v1/home";

  /**
   * [PUBLIC] Busca todos os dados da Home (Carousels e Stats).
   * Chama a rota pública otimizada por cache do backend.
   */
  public static async getHomeData(useMock: boolean): Promise<HomeDataResponse> {
    if (!useMock) {
      return apiClient.get<HomeDataResponse>(this.BASE_API);
    }

    return getMockHomeData();
  }

  // ==========================================
  // Operações Administrativas - Carousel
  // ==========================================

  /**
   * [ADMIN] Cria uma nova imagem/alerta para o Carousel da Home.
   */
  public static async createCarouselItem(
    data: CarouselCreatePayload,
    useMock: boolean,
  ): Promise<CarouselContent> {
    if (!useMock) {
      return apiClient.post<CarouselContent>(`${this.BASE_API_ADMIN}/carousel`, data);
    }

    return createMockCarouselItem(data);
  }

  /**
   * [ADMIN] Atualiza parcialmente um item do Carousel existente.
   */
  public static async updateCarouselItem(
    id: string,
    data: CarouselUpdatePayload,
    useMock: boolean,
  ): Promise<CarouselContent> {
    if (!useMock) {
      return apiClient.patch<CarouselContent>(
        `${this.BASE_API_ADMIN}/carousel/${id}`,
        data,
      );
    }

    return updateMockCarouselItem(id, data);
  }

  /**
   * [ADMIN] Deleta um item do Carousel da Home.
   */
  public static async deleteCarouselItem(
    id: string,
    useMock: boolean,
  ): Promise<void> {
    if (!useMock) {
      return apiClient.delete(`${this.BASE_API_ADMIN}/carousel/${id}`);
    }

    return deleteMockCarouselItem(id);
  }

  // ==========================================
  // Operações Administrativas - StatCards
  // ==========================================

  /**
   * [ADMIN] Cria um novo Card Estatístico para a Home.
   */
  public static async createStatCard(
    data: StatCardCreatePayload,
    useMock: boolean,
  ): Promise<StatCardContent> {
    if (!useMock) {
      return apiClient.post<StatCardContent>(`${this.BASE_API_ADMIN}/stats`, data);
    }

    return createMockStatCard(data);
  }

  /**
   * [ADMIN] Atualiza parcialmente um Card Estatístico existente.
   */
  public static async updateStatCard(
    id: string,
    data: StatCardUpdatePayload,
    useMock: boolean,
  ): Promise<StatCardContent> {
    if (!useMock) {
      return apiClient.patch<StatCardContent>(`${this.BASE_API_ADMIN}/stats/${id}`, data);
    }

    return updateMockStatCard(id, data);
  }

  /**
   * [ADMIN] Deleta um Card Estatístico da Home.
   */
  public static async deleteStatCard(
    id: string,
    useMock: boolean,
  ): Promise<void> {
    if (!useMock) {
      return apiClient.delete(`${this.BASE_API_ADMIN}/stats/${id}`);
    }

    return deleteMockStatCard(id);
  }
}
