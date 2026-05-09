import React, { useState } from 'react';
import { useHomeAdmin } from '../hooks/useHomeAdmin';
import { HomeAdminList } from './HomeAdminList';
import { CarouselForm } from './CarouselForm';
import { StatCardForm } from './StatCardForm';
import type { CarouselResponse, StatCardResponse, CarouselSaveDto, StatCardSaveDto } from '../types/homeAdmin.types';
import { Button } from '@/components/ui/Button/Button';

type ViewMode = 'list' | 'edit-carousel' | 'edit-stat';

export const HomeAdminManager: React.FC = () => {
  const { 
    carousels, 
    stats, 
    loading, 
    deleteCarousel, 
    saveCarousel, 
    saveStatCard,
    deleteStatCard // Assumindo que adicionamos no hook seguindo o mesmo padrão
  } = useHomeAdmin();

  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [editingCarousel, setEditingCarousel] = useState<CarouselResponse | undefined>(undefined);
  const [editingStat, setEditingStat] = useState<StatCardResponse | undefined>(undefined);

  // --- Handlers de Navegação Interna ---
  const handleOpenCarouselForm = (item?: CarouselResponse) => {
    setEditingCarousel(item);
    setViewMode('edit-carousel');
  };

  const handleOpenStatForm = (item?: StatCardResponse) => {
    setEditingStat(item);
    setViewMode('edit-stat');
  };

  const handleBackToList = () => {
    setEditingCarousel(undefined);
    setEditingStat(undefined);
    setViewMode('list');
  };

  // --- Handlers de Submissão ---
  const handleCarouselSubmit = async (data: CarouselSaveDto) => {
    await saveCarousel(data, editingCarousel?.id);
    handleBackToList();
  };

  const handleStatSubmit = async (data: StatCardSaveDto) => {
    await saveStatCard(data, editingStat?.id);
    handleBackToList();
  };

  return (
    <div className="home-admin-manager">
      {viewMode === 'list' && (
        <HomeAdminList
          carousels={carousels}
          stats={stats}
          isLoading={loading}
          onEditCarousel={handleOpenCarouselForm}
          onDeleteCarousel={deleteCarousel}
          onEditStat={handleOpenStatForm}
          onDeleteStat={deleteStatCard}
          // Passamos callbacks para os botões de "Novo" do cabeçalho da lista
          onCreateCarousel={() => handleOpenCarouselForm()}
          onCreateStat={() => handleOpenStatForm()}
        />
      )}

      {viewMode === 'edit-carousel' && (
        <div>
          <Button variant="secondary" size="sm" onClick={handleBackToList} style={{ marginBottom: '1rem' }}>
            &larr; Voltar para Lista
          </Button>
          <div className="form-wrapper" style={{ background: '#fff', padding: '1.5rem', borderRadius: '8px', border: '1px solid #e5e7eb' }}>
            <h2 style={{ marginBottom: '1.5rem' }}>{editingCarousel ? 'Editar Slide' : 'Novo Slide'}</h2>
            <CarouselForm 
              initialData={editingCarousel} 
              onSubmit={handleCarouselSubmit} 
              isLoading={loading} 
            />
          </div>
        </div>
      )}

      {viewMode === 'edit-stat' && (
        <div>
          <Button variant="secondary" size="sm" onClick={handleBackToList} style={{ marginBottom: '1rem' }}>
            &larr; Voltar para Lista
          </Button>
          <div className="form-wrapper" style={{ background: '#fff', padding: '1.5rem', borderRadius: '8px', border: '1px solid #e5e7eb' }}>
            <h2 style={{ marginBottom: '1.5rem' }}>{editingStat ? 'Editar Métrica' : 'Nova Métrica'}</h2>
            <StatCardForm 
              initialData={editingStat} 
              onSubmit={handleStatSubmit} 
              isLoading={loading} 
            />
          </div>
        </div>
      )}
    </div>
  );
};