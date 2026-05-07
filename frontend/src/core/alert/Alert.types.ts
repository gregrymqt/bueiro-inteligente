// src/core/alert/Alert.types.ts

// Adicionado o 'info'
export type AlertType = 'success' | 'error' | 'warning' | 'info';

export interface BaseAlertParams {
  title: string;
  text?: string;
}

export interface ConfirmAlertParams extends BaseAlertParams {
  onConfirm: () => void | Promise<void>;
  confirmButtonText?: string;
  cancelButtonText?: string;
}