import React, { useEffect, useState } from 'react';
import { MessageService } from '@/feature/messages/services/MessageService';
import type { UserMessage } from '@/feature/messages/types';
import { AlertService } from '@/core/alert/AlertService';
import { Button } from '@/components/ui/Button/Button';
import { Trash2, Check, Mail } from 'lucide-react';
import './MessageManagement.scss';

export const MessageManagement: React.FC = () => {
  const [messages, setMessages] = useState<UserMessage[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchMessages = async () => {
    setLoading(true);
    try {
      const data = await MessageService.getMessages(false); // Using mock for now
      setMessages(data);
    } catch {
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
      await MessageService.markAsRead(id, false);
      setMessages(messages.map(m => m.id === id ? { ...m, is_read: true } : m));
      AlertService.success('Mensagem marcada como lida.');
    } catch {
      AlertService.error('Erro', 'Não foi possível marcar a mensagem como lida.');
    }
  };

  const handleDelete = async (id: string) => {
    await AlertService.confirm({
      title: 'Confirmar exclusão',
      text: 'Tem certeza que deseja excluir esta mensagem?',
      onConfirm: async () => {
        try {
          await MessageService.deleteMessage(id, false);
          setMessages(messages.filter(m => m.id !== id));
        } catch {
          AlertService.error('Erro', 'Não foi possível excluir a mensagem.');
        }
      }
    });
  };

  if (loading) {
    return <div className="messages-loading">Carregando mensagens...</div>;
  }

  return (
    <div className="message-management-page">
      <header className="page-header">
        <h1>Mensagens de Contato</h1>
        <p>Acompanhe e responda às dúvidas enviadas pela Landing Page.</p>
      </header>

      <div className="messages-list">
        {messages.length === 0 ? (
          <p className="no-messages">Nenhuma mensagem encontrada.</p>
        ) : (
          messages.map(message => (
            <div key={message.id} className={`message-card ${!message.is_read ? 'unread' : ''}`}>
              <div className="message-header">
                <h3>{message.name}</h3>
                <span className="message-date">{new Date(message.created_at).toLocaleDateString()}</span>
              </div>
              <p className="message-email"><Mail size={14} /> {message.email}</p>
              <div className="message-body">
                <p>{message.message}</p>
              </div>
              <div className="message-actions">
                {!message.is_read && (
                  <Button type="button" variant="secondary" size="sm" onClick={() => handleMarkAsRead(message.id)} leftIcon={<Check size={14} />}>
                    Marcar como Lida
                  </Button>
                )}
                <Button type="button" variant="danger" size="sm" onClick={() => handleDelete(message.id)} leftIcon={<Trash2 size={14} />}>
                  Excluir
                </Button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
