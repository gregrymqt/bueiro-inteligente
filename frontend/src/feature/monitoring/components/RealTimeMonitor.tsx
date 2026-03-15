import React from 'react';
import { useDrainStatus } from '../hooks/useDrainStatus';
import { StatusBadge } from '@/components/ui/StatusBadge';
import './RealTimeMonitor.scss';

interface RealTimeMonitorProps {
  bueiroId: string;
  locationName?: string; // Opcional, para dar um nome amigável na UI
}

export const RealTimeMonitor: React.FC<RealTimeMonitorProps> = ({ 
  bueiroId,
  locationName = 'Bueiro - Terminal Piracicabana' 
}) => {
  // Chamamos o nosso Hook customizado com polling de 5 segundos
  const { data, loading, error, refetch } = useDrainStatus(bueiroId, 5000);

  // 1. Estado de Carregamento (Skeleton ou Spinner)
  if (loading) {
    return (
      <div className="monitor-card monitor-card--loading">
        <div className="spinner" aria-hidden="true"></div>
        <span>Sincronizando com o sensor...</span>
      </div>
    );
  }

  // 2. Estado de Erro (Falha no Redis ou na API)
  if (error) {
    return (
      <div className="monitor-card monitor-card--error">
        <p className="error-message">Falha de comunicação: {error}</p>
        <button onClick={refetch} className="btn-retry">Tentar Novamente</button>
      </div>
    );
  }

  // Fallback de segurança caso os dados venham nulos mesmo sem erro
  if (!data) return null;

  // 3. Estado de Sucesso (Renderização dos Dados)
  return (
    <section className="monitor-card">
      <header className="monitor-card__header">
        <div className="monitor-card__info">
          <h2 className="monitor-card__title">{locationName}</h2>
          <span className="monitor-card__id">ID: {data.id_bueiro}</span>
        </div>
        
        {/* Aqui usamos o nosso Dumb Component, passando o dado tipado do Pydantic */}
        <StatusBadge status={data.status} />
      </header>

      <div className="monitor-card__metrics">
        {/* Aqui poderíamos extrair para um componente <DataCard />, 
            mas mantive interno para simplificar a visualização da feature */}
        <div className="metric">
          <span className="metric__label">Nível de Obstrução</span>
          <strong className="metric__value">{data.nivel_obstrucao}%</strong>
        </div>
        
        <div className="metric">
          <span className="metric__label">Distância (Superfície)</span>
          <strong className="metric__value">{data.distancia_cm} cm</strong>
        </div>
      </div>

      <footer className="monitor-card__footer">
        {/* Formatando a data ISO que vem do Python para o horário local */}
        <span>Última leitura: {new Date(data.ultima_atualizacao).toLocaleTimeString('pt-BR')}</span>
      </footer>
    </section>
  );
};