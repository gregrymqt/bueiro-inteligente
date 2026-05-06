// Baseado na documentação técnica do Mercado Pago
export interface CardData {
  token: string;
  issuer_id: string;
  payment_method_id: string;
  transaction_amount: number;
  payment_method_option_id?: string; // Trocado null por undefined via '?'
  processing_mode?: string;          // Trocado null por undefined via '?'
  installments: number;
  payer: {
    email: string;
    identification: {
      type: string;
      number: string;
    };
  };
}

export interface AdditionalData {
  bin: string;
  lastFourDigits: string;
  cardholderName: string;
  paymentTypeId: string;
}

/** 
 * Payload para o backend .NET 8[cite: 25, 32]
 */
export interface CreditCardRequest {
  amount: number;
  description: string;
  payerEmail: string;
  token: string;
  paymentMethodId: string;
  installments: number;
  planId?: string;
}

export interface CreditCardResponse {
  orderId: string;
  paymentId: number;
  status: string;
  statusDetail: string;
  externalResourceUrl?: string;
  externalReference: string;
}