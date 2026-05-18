export const PaymentMethodType = {
  PIX: 'pix',
  CREDIT_CARD: 'credit_card',
  MERCADO_PAGO: 'mercado_pago_wallet',
} as const;

export type PaymentMethodType = (typeof PaymentMethodType)[keyof typeof PaymentMethodType];

export interface PaymentOption {
  id: PaymentMethodType;
  title: string;
  description: string;
  icon: string;
  badge?: string; // Ex: "Aprovação Instantânea"
}