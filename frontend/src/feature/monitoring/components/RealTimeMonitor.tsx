import React from 'react';
import { useDrainStatus } from '../hooks/useDrainStatus'; // O Hook que usa o Service/WS
import { StatusBadge } from '@/components/ui/StatusBadge';
import './RealTimeMonitor.scss';

interface RealTimeMonitorProps {
  bueiroId: string;
  locationName?: string;
}

export const RealTimeMonitor: React.FC<RealTimeMonitorProps> = ({ 
  bueiroId,
  locationName = 'Terminal Piracicabana' 
}) => {
  // Agora o hook gerencia o fetch inicial + WebSocket automaticamente
  const { data, loading, error, refetch } = useDrainStatus(bueiroId);

  if (loading) return (
    <div className="monitor-card monitor-card--loading">
      <div className="spinner"></div>
      <span>Sincronizando telemetria...</span>
    </div>
  );

  if (error) return (
    <div className="monitor-card monitor-card--error">
      <p>Erro de conexão: {error}</p>
      <button type="button" onClick={refetch} className="btn-retry">Tentar Novamente</button>
    </div>
  );

  return (
    <section className="monitor-card">
      <header className="monitor-card__header">
        <div className="monitor-card__info">
          <h2 className="monitor-card__title">{locationName}</h2>
          <span className="monitor-card__id">ID: {data?.id_bueiro}</span>
        </div>
        {/* Passa o status vindo direto do Pydantic (NORMAL, ALERTA, CRITICO) */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <button
            type="button"
            onClick={refetch}
            className="btn-refresh"
            aria-label="Atualizar dados manualmente"
          >
            <svg className={loading ? "spin-animation" : ""} width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          </button>
          <StatusBadge status={data?.status || 'DESCONHECIDO'} />
        </div>
      </header>

      <div className="monitor-card__metrics">
        <div className="metric">
          <span className="metric__label">Nível de Obstrução</span>
          <strong className="metric__value">{data?.nivel_obstrucao}%</strong>
        </div>
        <div className="metric">
          <span className="metric__label">Distância Interna</span>
          <strong className="metric__value">{data?.distancia_cm} cm</strong>
        </div>
      </div>

      <footer className="monitor-card__footer">
        <span>🕒 Atualizado em: {new Date(data?.ultima_atualizacao || '').toLocaleTimeString()}</span>
      </footer>
    </section>
  );
};