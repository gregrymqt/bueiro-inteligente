import React, { useState } from 'react';
import { useDrainsList } from '../hooks/useDrainsList';
import { RealTimeMonitor } from './RealTimeMonitor';
import './LiveMonitorContainer.scss';

export const LiveMonitorContainer: React.FC = () => {
    const { data: drains, loading, error, refetch } = useDrainsList();
    const [explicitDrainId, setExplicitDrainId] = useState<string | null>(null);

    // ✅ CORREÇÃO 1: Usando hardware_id exatamente como definido na interface
    const selectedDrainId = explicitDrainId || (drains.length > 0 ? drains[0].hardware_id : '');

    if (loading) {
        return (
            <div className="live-monitor-container live-monitor-container--loading">
                <div className="spinner"></div>
                <span>Carregando seus dispositivos...</span>
            </div>
        );
    }

    if (error) {
        return (
            <div className="live-monitor-container live-monitor-container--error">
                <p>Não foi possível carregar a lista de bueiros: {error}</p>
                <button type="button" onClick={refetch} className="btn-retry">Tentar Novamente</button>
            </div>
        );
    }

    if (drains.length === 0) {
        return (
            <div className="live-monitor-container live-monitor-container--empty">
                <h2>Nenhum bueiro encontrado</h2>
                <p>Você ainda não possui bueiros cadastrados para monitoramento.</p>
                <button type="button" onClick={refetch} className="btn-retry">Atualizar</button>
            </div>
        );
    }

    // ✅ CORREÇÃO 2: Verificação do find também usando hardware_id
    const selectedDrain = drains.find(d => d.hardware_id === selectedDrainId);

    return (
        <div className="live-monitor-container">
            <div className="live-monitor-container__header">
                <label htmlFor="drain-select">Selecione o Dispositivo:</label>
                <select
                    id="drain-select"
                    value={selectedDrainId}
                    onChange={(e) => setExplicitDrainId(e.target.value)}
                    className="drain-select-dropdown"
                >
                    {/* ✅ CORREÇÃO 3: Sintaxe JSX limpa para o loop funcionar perfeitamente */}
                    {drains.map(drain => (
                        <option key={drain.id} value={drain.hardware_id}>
                            {drain.name} - {drain.hardware_id}
                        </option>
                    ))}
                </select>
            </div>

            {selectedDrainId && (
                <RealTimeMonitor
                    key={selectedDrainId} 
                    bueiroId={selectedDrainId} 
                    locationName={selectedDrain?.name || 'Local Desconhecido'}
                />
            )}
        </div>
    );
};