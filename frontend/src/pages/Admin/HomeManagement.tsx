import React, { useState } from 'react';
import { useHomeAdmin } from '@/feature/home/hooks/useHomeAdmin';
import { CarouselForm } from '@/feature/home/components/admin/CarouselForm';
import { StatCardForm } from '@/feature/home/components/admin/StatCardForm';
import type { CarouselContent, StatCardContent } from '@/feature/home/types';
import './HomeManagement.scss';

export const HomeManagement: React.FC = () => {
  const {
    carousels,
    stats,
    loading,
    refreshData,
    removeBanner,
    removeStatCard
  } = useHomeAdmin();

  const [notification, setNotification] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  // Estados de formulário (null = fechado, undefined/objeto vazio ou completo = aberto)
  const [activeCarouselForm, setActiveCarouselForm] = useState<{ isEditing: boolean, data?: CarouselContent } | null>(null);
  const [activeStatCardForm, setActiveStatCardForm] = useState<{ isEditing: boolean, data?: StatCardContent } | null>(null);

  const showNotification = (message: string, type: 'success' | 'error' = 'success') => {
    setNotification({ message, type });
    setTimeout(() => setNotification(null), 3000);
  };

  const handleDeleteCarousel = async (id: string) => {
    if (!window.confirm('Tem certeza que deseja excluir este banner?')) return;
    const success = await removeBanner(id);
    if (success) showNotification('Banner excluído com sucesso!');
    else showNotification('Erro ao excluir o banner.', 'error');
  };

  const handleDeleteStatCard = async (id: string) => {
    if (!window.confirm('Tem certeza que deseja excluir esta estatística?')) return;
    const success = await removeStatCard(id);
    if (success) showNotification('Estatística excluída com sucesso!');
    else showNotification('Erro ao excluir estatística.', 'error');
  };

  return (
    <div className="home-management-page">
      <div className="page-header">
        <h1>Gerenciamento da Home</h1>
      </div>

      {notification && (
        <div className={`notification ${notification.type}`}>
          {notification.message}
        </div>
      )}

      {loading ? (
        <div className="loading-container">Carregando dados...</div>
      ) : (
        <>
          {/* Seção de Banners / Carousel */}
          <section className="admin-section">
            <div className="section-header">
              <h2>Banners do Carrossel</h2>
              {!activeCarouselForm && (
                <button 
                  className="btn-add" 
                  onClick={() => setActiveCarouselForm({ isEditing: false })}
                >
                  Adicionar Novo
                </button>
              )}
            </div>

            {activeCarouselForm ? (
              <CarouselForm 
                initialData={activeCarouselForm.data} 
                onSuccess={() => {
                  setActiveCarouselForm(null);
                  showNotification(activeCarouselForm.isEditing ? 'Banner atualizado!' : 'Banner criado!');
                  refreshData(); // Atualiza a lista com as mudanças feitas pelo form
                }}
                onCancel={() => setActiveCarouselForm(null)}
              />
            ) : (
              <div className="content-grid">
                {carousels.length === 0 ? (
                  <p>Nenhum banner cadastrado.</p>
                ) : (
                  carousels.map((banner) => (
                    <div className="item-card" key={banner.id}>
                      <h3 className="card-title">{banner.title}</h3>
                      <p className="card-info">Sessão: <span>{banner.section}</span></p>
                      <p className="card-info">Ordem: <span>{banner.order}</span></p>
                      <div className="card-actions">
                        <button 
                          className="btn-edit" 
                          onClick={() => setActiveCarouselForm({ isEditing: true, data: banner })}
                        >
                          Editar
                        </button>
                        <button 
                          className="btn-delete" 
                          onClick={() => handleDeleteCarousel(banner.id)}
                        >
                          Excluir
                        </button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}
          </section>

          {/* Seção de Cards de Estatística */}
          <section className="admin-section">
            <div className="section-header">
              <h2>Cards de Estatística</h2>
              {!activeStatCardForm && (
                <button 
                  className="btn-add" 
                  onClick={() => setActiveStatCardForm({ isEditing: false })}
                >
                  Adicionar Novo
                </button>
              )}
            </div>

            {activeStatCardForm ? (
              <StatCardForm 
                initialData={activeStatCardForm.data}
                onSuccess={() => {
                  setActiveStatCardForm(null);
                  showNotification(activeStatCardForm.isEditing ? 'Estatística atualizada!' : 'Estatística criada!');
                  refreshData(); // Atualiza a lista com as mudanças feitas pelo form
                }}
                onCancel={() => setActiveStatCardForm(null)}
              />
            ) : (
              <div className="content-grid">
                {stats.length === 0 ? (
                  <p>Nenhuma estatística cadastrada.</p>
                ) : (
                  stats.map((stat) => (
                    <div className="item-card" key={stat.id}>
                      <h3 className="card-title">{stat.title}</h3>
                      <p className="card-info">Valor: <span>{stat.value}</span></p>
                      <p className="card-info">Cor: <span>{stat.color}</span></p>
                      <p className="card-info">Ícone: <span>{stat.icon_name}</span></p>
                      <p className="card-info">Ordem: <span>{stat.order}</span></p>
                      <div className="card-actions">
                        <button 
                          className="btn-edit" 
                          onClick={() => setActiveStatCardForm({ isEditing: true, data: stat })}
                        >
                          Editar
                        </button>
                        <button 
                          className="btn-delete" 
                          onClick={() => handleDeleteStatCard(stat.id)}
                        >
                          Excluir
                        </button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}
          </section>
        </>
      )}
    </div>
  );
};
