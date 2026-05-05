import { useEffect } from 'react';
import { signalRClient } from '@/core/socket/SignalRClient'; // Ajuste o path se necessário[cite: 30]
import { AlertService } from '@/core/alert/AlertService';

// Contrato idêntico ao NotificationResponseDTO do seu Backend (C#)
export interface NotificationPayload {
  id: string;
  title: string;
  message: string;
  type: 'Info' | 'Success' | 'Error' | 'Warning';
  isRead: boolean;
  createdAt: string;
}

export function useNotifications() {
  useEffect(() => {
    // Inscreve-se no evento disparado pelo seu NotificationService no backend
    // Lembre-se que no C# você configurou "new_notification" em snake_case[cite: 1, 12]
    const unsubscribe = signalRClient.subscribe<NotificationPayload>('new_notification', (payload) => {
      
      // Mapeamento visual baseado no tipo da notificação que veio do C#
      switch (payload.type) {
        case 'Success':
          AlertService.success(payload.title, payload.message);
          break;
        case 'Error':
          AlertService.error(payload.title, payload.message);
          break;
        case 'Warning':
          AlertService.warning(payload.title, payload.message);
          break;
        case 'Info':
        default:
          AlertService.info(payload.title, payload.message);
          break;
      }

      const event = new CustomEvent('badge:update', { detail: payload });
      window.dispatchEvent(event);
    });

    // Cleanup: Desconecta ao desmontar o componente[cite: 28, 30]
    return () => {
      unsubscribe();
    };
  }, []);
}