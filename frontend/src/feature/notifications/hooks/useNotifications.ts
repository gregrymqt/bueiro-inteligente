import { useEffect } from 'react';
import { signalRClient } from '@/core/socket/SignalRClient'; //
import { AlertService } from '@/core/alert/AlertService';

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
    const unsubscribe = signalRClient.subscribe<NotificationPayload>('new_notification', (payload) => {
      
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

      // ==========================================
      // NOVA LÓGICA: RENOVAÇÃO DE SESSÃO PÓS-PAGAMENTO
      // ==========================================
      if (payload.title === 'Pagamento Aprovado! 🎉') {
        // Aguardamos 3.5 segundos para o usuário ler a notificação de sucesso original
        setTimeout(() => {
          AlertService.info(
            'Conta Atualizada! 🚀', 
            'Seu plano foi ativado com sucesso. Vamos atualizar sua sessão para liberar a Gestão de Bueiros.'
          ).then(() => {
            // Ao confirmar o alerta, disparamos o evento que o seu AuthInterceptor.tsx já escuta!
            // Isso vai limpar o token antigo e redirecionar para o /login com segurança.
            window.dispatchEvent(new Event('auth:unauthorized'));
          });
        }, 3500); 
      }
      // ==========================================

      const event = new CustomEvent('badge:update', { detail: payload });
      window.dispatchEvent(event);
    });

    // Cleanup: Desconecta ao desmontar o componente
    return () => {
      unsubscribe();
    };
  }, []);
}