// feature/home/components/HomeAdminList.tsx
import React from 'react';
import { Edit, Trash2, Image as ImageIcon } from 'lucide-react';
import type { CarouselResponse, StatCardResponse } from '../types/homeAdmin.types';
import styles from './HomeAdminList.module.scss';
import { Button } from '@/components/ui/Button/Button';
import { StatusBadge } from '@/components/ui/Status/StatusBadge';
import { GenericTable } from '@/components/ui/Table/GenericTable';
import type { Column } from '@/components/ui/Table/types/index.types';
import { ImageResolver } from '@/core/http/ImageResolver';

interface HomeAdminListProps {
  carousels: CarouselResponse[];
  stats: StatCardResponse[];
  isLoading: boolean;
  onEditCarousel: (item: CarouselResponse) => void;
  onDeleteCarousel: (id: string) => void;
  onEditStat: (item: StatCardResponse) => void;
  onDeleteStat: (id: string) => void;
  onCreateCarousel: () => void;
  onCreateStat: () => void;
}

export const HomeAdminList: React.FC<HomeAdminListProps> = ({
  carousels,
  stats,
  isLoading,
  onEditCarousel,
  onDeleteCarousel,
  onEditStat,
  onDeleteStat,
  onCreateCarousel,
  onCreateStat
}) => {

  const carouselColumns: Column<CarouselResponse>[] = [
    {
      key: 'desktop_image_url',
      label: 'Preview',
      render: (url: string) => {
        // Resolvemos o caminho da imagem de forma segura antes de passar para a tag img
        const resolvedUrl = ImageResolver.resolve(url);
        return (
          <div className={styles.thumbnail}>
            {url ? <img src={resolvedUrl} alt="Preview" /> : <ImageIcon size={20} />}
          </div>
        );
      }
    },
    { key: 'title', label: 'Título' },
    { 
      key: 'section', 
      label: 'Seção',
      render: (value: string) => <StatusBadge status={value} />
    },
    { key: 'order', label: 'Ordem' },
    {
      key: 'actions',
      label: 'Ações',
      render: (_, item) => (
        <div className={styles.actions}>
          <Button variant="secondary" size="sm" onClick={() => onEditCarousel(item)}>
            <Edit size={16} />
          </Button>
          <Button variant="danger" size="sm" onClick={() => onDeleteCarousel(item.id)}>
            <Trash2 size={16} />
          </Button>
        </div>
      )
    }
  ];

  const statColumns: Column<StatCardResponse>[] = [
    { 
      key: 'icon_name', 
      label: 'Ícone',
      render: (iconName: string) => <div className={styles.iconCircle}>{iconName}</div>
    },
    { key: 'title', label: 'Título' },
    { key: 'value', label: 'Valor' },
    { 
      key: 'color', 
      label: 'Cor',
      render: (color: string) => <StatusBadge status={color} />
    },
    {
      key: 'actions',
      label: 'Ações',
      render: (_, item) => (
        <div className={styles.actions}>
          <Button variant="secondary" size="sm" onClick={() => onEditStat(item)}>
            <Edit size={16} />
          </Button>
          <Button variant="danger" size="sm" onClick={() => onDeleteStat(item.id)}>
            <Trash2 size={16} />
          </Button>
        </div>
      )
    }
  ];

  return (
    <div className={styles.adminListContainer}>
      <section className={styles.section}>
        <div className={styles.header}>
          <h2>Gestão do Carrossel (Hero)</h2>
          <Button size="sm" onClick={onCreateCarousel}>Novo Slide</Button>
        </div>
        <GenericTable 
          data={carousels} 
          columns={carouselColumns} 
          isLoading={isLoading} 
          emptyMessage="Nenhum slide cadastrado para a Home."
        />
      </section>

      <section className={styles.section}>
        <div className={styles.header}>
          <h2>Cards de Estatísticas</h2>
          <Button size="sm" onClick={onCreateStat}>Novo Card</Button>
        </div>
        <GenericTable 
          data={stats} 
          columns={statColumns} 
          isLoading={isLoading}
          emptyMessage="Nenhum card de estatística configurado."
        />
      </section>
    </div>
  );
};