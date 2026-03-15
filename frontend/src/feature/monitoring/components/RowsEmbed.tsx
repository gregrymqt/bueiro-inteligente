import React, { useState } from 'react';
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
  // Estado para controlar se o iframe do Rows já terminou de carregar
  const [isLoading, setIsLoading] = useState<boolean>(true);

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