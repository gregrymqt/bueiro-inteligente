export type AlertType = 'success' | 'error' | 'warning';

export interface BaseAlertParams {
  title: string;
  text?: string;
}

export interface ConfirmAlertParams extends BaseAlertParams {
  onConfirm: () => void | Promise<void>;
  confirmButtonText?: string;
  cancelButtonText?: string;
}
