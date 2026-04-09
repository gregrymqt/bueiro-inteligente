import Swal from 'sweetalert2';
import styles from './Alert.module.scss';

export class AlertService {
  /**
   * Mostra um alerta verde de confirmação que fecha sozinho.
   */
  public static success(title: string, message: string): void {
    Swal.fire({
      icon: 'success',
      title,
      text: message,
      timer: 3000,
      timerProgressBar: true,
      showConfirmButton: false,
      customClass: {
        popup: styles.alertPopup,
        title: styles.alertTitle,
      },
    });
  }

  /**
   * Mostra um alerta vermelho para falhas e erros.
   */
  public static error(title: string, message: string): void {
    Swal.fire({
      icon: 'error',
      title,
      text: message,
      confirmButtonText: 'Fechar',
      customClass: {
        popup: styles.alertPopup,
        title: styles.alertTitle,
        confirmButton: `${styles.alertConfirmButton} ${styles.alertButtonError}`,
      },
      buttonsStyling: false,
    });
  }

  /**
   * Mostra um alerta de aviso e pede confirmação antes de executar uma ação.
   */
  public static async confirm(
    title: string,
    text: string,
    onConfirm: () => void | Promise<void>
  ): Promise<void> {
    const result = await Swal.fire({
      icon: 'warning',
      title,
      text,
      showCancelButton: true,
      confirmButtonText: 'Sim',
      cancelButtonText: 'Cancelar',
      customClass: {
        popup: styles.alertPopup,
        title: styles.alertTitle,
        confirmButton: `${styles.alertConfirmButton} ${styles.alertButtonWarning}`,
        cancelButton: styles.alertCancelButton,
      },
      buttonsStyling: false,
      reverseButtons: true, // Coloca o botão Cancelar à esquerda
    });

    if (result.isConfirmed) {
      await onConfirm();
    }
  }

  /**
   * Alerta genérico de warning.
   */
  public static warning(title: string, message: string): void {
     Swal.fire({
       icon: 'warning',
       title,
       text: message,
       confirmButtonText: 'OK',
       customClass: {
         popup: styles.alertPopup,
         title: styles.alertTitle,
         confirmButton: `${styles.alertConfirmButton} ${styles.alertButtonWarning}`,
       },
       buttonsStyling: false,
     });
   }
}