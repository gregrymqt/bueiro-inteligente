export interface Feedback {
  id: string;
  userName: string;
  role: string;
  comment: string;
  rating: number;
  avatarUrl?: string;
  createdAt: string;
}

export interface FeedbackCreatePayload {
  comment: string;
  rating: number;
}

// Payload para atualização parcial[cite: 39]
export type FeedbackUpdatePayload = Partial<FeedbackCreatePayload>;