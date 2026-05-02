import { apiClient } from "@/core/http/ApiClient";
import type { Feedback, FeedbackCreatePayload, FeedbackUpdatePayload } from "../types";

export class FeedbackService {
  private static readonly BASE_API = "/api/v1/feedbacks";

  public static async getFeedbacks(): Promise<Feedback[]> {
    return apiClient.get<Feedback[]>(this.BASE_API);
  }

  public static async submitFeedback(payload: FeedbackCreatePayload): Promise<Feedback> {
    return apiClient.post<Feedback>(this.BASE_API, payload);
  }

  /**
   * [AUTH] Atualiza um feedback existente. 
   * O backend .NET deve retornar o feedback para re-moderação[cite: 10, 40].
   */
  public static async updateFeedback(id: string, payload: FeedbackUpdatePayload): Promise<Feedback> {
    return apiClient.patch<Feedback>(`${this.BASE_API}/${id}`, payload);
  }

  /**
   * [AUTH] Remove um feedback permanentemente[cite: 40].
   */
  public static async deleteFeedback(id: string): Promise<void> {
    return apiClient.delete(`${this.BASE_API}/${id}`);
  }
}