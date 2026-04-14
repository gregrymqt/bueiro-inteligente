import React, { useMemo, useState } from 'react';
import { mockRowsInsights, mockRowsTrendPoints } from '../mocks/monitoringMocks';
import './RowsEmbed.scss'; // Certifique-se de criar este arquivo CSS para os estilos

interface RowsEmbedProps {
  embedUrl: string;
  title: string;
  height?: number | string; // Permitindo flexibilidade na altura
}

export const RowsEmbed: React.FC<RowsEmbedProps> = ({ 
  embedUrl, 
  title, 
  height = '500px' 
}) => {
  const normalizedUrl = embedUrl.trim().toLowerCase();

  const shouldRenderMockPanel = useMemo(() => {
    return normalizedUrl.includes('sua-planilha-aqui') || normalizedUrl.startsWith('mock:');
  }, [embedUrl]);

  const shouldRenderEmptyState = normalizedUrl.length === 0;

  // Estado para controlar se o iframe do Rows já terminou de carregar
  const [isLoading, setIsLoading] = useState<boolean>(!shouldRenderMockPanel);

  const maxTrendValue = useMemo(() => {
    return Math.max(...mockRowsTrendPoints.map((point) => point.value), 1);
  }, []);

  if (shouldRenderEmptyState) {
    return (
      <div className="embed-wrapper embed-wrapper--empty" style={{ height, position: 'relative', width: '100%' }}>
        <div className="embed-empty">
          <p className="embed-empty__eyebrow">Rows não configurado</p>
          <h3 className="embed-empty__title">{title}</h3>
          <p className="embed-empty__text">
            Defina <strong>VITE_ROWS_EMBED_URL</strong> para exibir o painel analítico real nesta área.
          </p>
        </div>
      </div>
    );
  }

  if (shouldRenderMockPanel) {
    return (
      <div className="embed-wrapper embed-wrapper--mock" style={{ height, position: 'relative', width: '100%' }}>
        <div className="embed-fallback">
          <header className="embed-fallback__header">
            <div>
              <p className="embed-fallback__eyebrow">Rows mockado</p>
              <h3 className="embed-fallback__title">{title}</h3>
            </div>
            <span className="embed-fallback__badge">Dados locais</span>
          </header>

          <div className="embed-fallback__insights">
            {mockRowsInsights.map((insight) => (
              <article key={insight.label} className={`embed-fallback__insight embed-fallback__insight--${insight.tone}`}>
                <span className="embed-fallback__metric-label">{insight.label}</span>
                <strong className="embed-fallback__metric-value">{insight.value}</strong>
                <p className="embed-fallback__metric-detail">{insight.detail}</p>
              </article>
            ))}
          </div>

          <section className="embed-fallback__chart">
            <div className="embed-fallback__chart-header">
              <span>Tendência das leituras</span>
              <span>Últimas 6 janelas</span>
            </div>

            <div className="embed-fallback__bars">
              {mockRowsTrendPoints.map((point) => {
                const percentageHeight = Math.max((point.value / maxTrendValue) * 100, 12);

                return (
                  <div key={point.label} className="embed-fallback__bar-group">
                    <div className="embed-fallback__bar-track">
                      <div
                        className="embed-fallback__bar"
                        style={{ height: `${percentageHeight}%` }}
                        aria-hidden="true"
                      />
                    </div>
                    <strong className="embed-fallback__bar-value">{point.value}%</strong>
                    <span className="embed-fallback__bar-label">{point.label}</span>
                  </div>
                );
              })}
            </div>
          </section>
        </div>
      </div>
    );
  }

  return (
    <div className="embed-wrapper" style={{ height, position: 'relative', width: '100%' }}>
      
      {/* Exibe o seu Loader/Spinner customizado enquanto carrega */}
      {isLoading && (
        <div className="embed-loader">
          <span>Carregando painel analítico...</span>
        </div>
      )}

      <iframe
        src={embedUrl}
        title={title}
        width="100%"
        height="100%"
        frameBorder="0"
        onLoad={() => setIsLoading(false)} // A mágica acontece aqui
        style={{ 
          opacity: isLoading ? 0 : 1, 
          transition: 'opacity 0.3s ease-in-out' 
        }}
      />
    </div>
  );
};