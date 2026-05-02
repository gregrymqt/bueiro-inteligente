import React, { useState, useEffect } from 'react';
import { MessageService } from '@/feature/message/services/MessageService';
import type { ContactMessage } from '@/feature/message/types';
import { AlertService } from '@/core/alert/AlertService';
import { Button } from '@/components/ui/Button/Button';
import { Trash2, CheckCircle } from 'lucide-react';
import styles from './MessageManagement.module.scss';

export const MessageManagement: React.FC = () => {
  const [messages, setMessages] = useState<ContactMessage[]>([]);
  const [loading, setLoading] = useState(true);

  const useMock = true;

  const fetchMessages = async () => {
    setLoading(true);
    try {
      const data = await MessageService.getMessages(useMock);
      setMessages(data);
    } catch (error) {
      AlertService.error('Erro', 'Não foi possível carregar as mensagens.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void fetchMessages();
  }, []);

  const handleMarkAsRead = async (id: string) => {
    try {
      await MessageService.markAsRead(id, useMock);
      setMessages((prev) =>
        prev.map(msg => msg.id === id ? { ...msg, isRead: true } : msg)
      );
      AlertService.success('Mensagem marcada como lida.');
    } catch (error) {
      AlertService.error('Erro', 'Não foi possível marcar a mensagem como lida.');
    }
  };

  const handleDelete = async (id: string) => {
    await AlertService.confirm({
      title: 'Confirmar exclusão',
      text: 'Tem certeza que deseja excluir esta mensagem?',
      onConfirm: async () => {
        try {
          await MessageService.deleteMessage(id, useMock);
          setMessages((prev) => prev.filter(msg => msg.id !== id));
          AlertService.success('Mensagem excluída com sucesso.');
        } catch (error) {
          AlertService.error('Erro', 'Não foi possível excluir a mensagem.');
        }
      }
    });
  };

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Painel administrativo</p>
          <h1 className={styles.title}>Gerenciamento de Mensagens</h1>
          <p className={styles.subtitle}>
            Visualize e gerencie as mensagens de contato recebidas.
          </p>
        </div>
      </header>

      <div className={styles.content}>
        {loading ? (
          <div className={styles.state}>Carregando mensagens...</div>
        ) : messages.length === 0 ? (
          <div className={styles.state}>Nenhuma mensagem recebida.</div>
        ) : (
          <div className={styles.grid}>
            {messages.map((msg) => (
              <article key={msg.id} className={`${styles.card} ${!msg.isRead ? styles.unread : ''}`}>
                <div className={styles.cardHeader}>
                  <h3 className={styles.subject}>{msg.subject}</h3>
                  <span className={styles.date}>{new Date(msg.createdAt).toLocaleDateString()}</span>
                </div>
                <div className={styles.author}>
                  <strong>{msg.name}</strong> ({msg.email})
                </div>
                <p className={styles.messageText}>{msg.message}</p>
                <div className={styles.actions}>
                  {!msg.isRead && (
                    <Button
                      type="button"
                      variant="secondary"
                      size="sm"
                      leftIcon={<CheckCircle size={16} />}
                      onClick={() => handleMarkAsRead(msg.id)}
                    >
                      Marcar como Lida
                    </Button>
                  )}
                  <Button
                    type="button"
                    variant="danger"
                    size="sm"
                    leftIcon={<Trash2 size={16} />}
                    onClick={() => handleDelete(msg.id)}
                  >
                    Excluir
                  </Button>
                </div>
              </article>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
