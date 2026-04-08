import Swal from 'sweetalert2';
import styles from './Alert.module.scss';
import type { ConfirmAlertParams } from './Alert.types';

export class AlertService {
  /**
   * Quick success message with a timer.
   */
  static success(title: string, text?: string): void {
    Swal.fire({
      icon: 'success',
      title,
      text,
      timer: 2000,
      showConfirmButton: false,
      customClass: {
        popup: styles.popup,
        title: styles.title,
      },
    });
  }

  /**
   * Error message (e.g., API errors) requiring user acknowledgment.
   */
  static error(title: string, text?: string): void {
    Swal.fire({
      icon: 'error',
      title,
      text,
      confirmButtonText: 'OK',
      customClass: {
        popup: styles.popup,
        title: styles.title,
        confirmButton: styles.errorButton,
      },
    });
  }

  /**
   * Warning/confirmation dialog executing a callback if confirmed.
   */
  static async confirm({
    title,
    text,
    onConfirm,
    confirmButtonText = 'Confirmar',
    cancelButtonText = 'Cancelar',
  }: ConfirmAlertParams): Promise<void> {
    const result = await Swal.fire({
      icon: 'warning',
      title,
      text,
      showCancelButton: true,
      confirmButtonText,
      cancelButtonText,
      reverseButtons: true,
      customClass: {
        popup: styles.popup,
        title: styles.title,
        confirmButton: styles.warningButton,
        cancelButton: styles.cancelButton,
      },
    });

    if (result.isConfirmed) {
      await onConfirm();
    }
  }
}
