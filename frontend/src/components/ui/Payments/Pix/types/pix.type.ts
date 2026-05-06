/**
 * Payload para criação de uma ordem de pagamento via Pix
 */
export interface CreatePixRequest {
  amount: number;
  description: string;
  payerEmail: string;
  firstName: string;
  lastName: string;
  identificationType: string; // CPF ou CNPJ[cite: 13]
  identificationNumber: string;
  planId?: string; // Guid opcional para assinatura de plano[cite: 13]
}

/**
 * Resposta contendo os dados do QR Code e metadados da transação[cite: 13]
 */
export interface PixPaymentResponse {
  orderId: string;
  paymentId: number;
  status: string;
  statusDetail: string;
  qrCode: string; // Pix Copia e Cola[cite: 13]
  qrCodeBase64: string; // Imagem do QR Code em Base64[cite: 13]
  ticketUrl: string; // Link externo para instruções[cite: 13]
  expirationDate: string; // ISO Date String
  externalReference: string; // ID da transação no banco (Guid)[cite: 13]
}

/**
 * Payload para retentativa de uma transação Pix que falhou[cite: 13]
 */
export interface RetryPixRequest {
  orderId: string;
  transactionId: string;
}