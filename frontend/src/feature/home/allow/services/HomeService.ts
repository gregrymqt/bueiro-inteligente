// feature/home/services/HomeService.ts
import { apiClient } from "@/core/http/ApiClient";
import type { CarouselResponse, StatCardResponse } from "../../admin/types/homeAdmin.types";

export interface HomePublicResponse {
  carousels: CarouselResponse[];
  stats: StatCardResponse[];
}

export class HomeService {
  private static readonly BASE_API = "/api/v1/home";

  public static async getLandingPageData(): Promise<HomePublicResponse> {
    return apiClient.get<HomePublicResponse>(this.BASE_API);
  }
}