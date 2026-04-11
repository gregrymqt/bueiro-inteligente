import { Button } from '@/components/ui/Button/Button';
import { PencilLine, Trash2 } from 'lucide-react';
import type { Drain } from '../types';
import styles from './DrainList.module.scss';

interface DrainListProps {
  drains: Drain[];
  loading?: boolean;
  isSaving?: boolean;
  selectedDrainId?: string;
  onEdit: (drain: Drain) => void;
  onDelete: (drain: Drain) => void;
}

export const DrainList = ({
  drains,
  loading = false,
  isSaving = false,
  selectedDrainId,
  onEdit,
  onDelete,
}: DrainListProps) => {
  return (
    <section className={styles.card}>
      <div className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Inventário operacional</p>
          <h2 className={styles.title}>Bueiros cadastrados</h2>
        </div>

        <span className={styles.counter}>{drains.length} itens</span>
      </div>

      {loading ? (
        <div className={styles.state}>Carregando bueiros...</div>
      ) : drains.length === 0 ? (
        <div className={styles.state}>Nenhum bueiro cadastrado ainda.</div>
      ) : (
        <div className={styles.grid}>
          {drains.map((drain) => {
            const isSelected = drain.id === selectedDrainId;

            return (
              <article key={drain.id} className={`${styles.itemCard} ${isSelected ? styles.selected : ''}`}>
                <div className={styles.itemHeader}>
                  <div>
                    <h3 className={styles.itemTitle}>{drain.name}</h3>
                    <p className={styles.itemSubtitle}>{drain.address}</p>
                  </div>

                  <span className={`${styles.statusBadge} ${drain.is_active ? styles.active : styles.inactive}`}>
                    {drain.is_active ? 'Ativo' : 'Inativo'}
                  </span>
                </div>

                <dl className={styles.details}>
                  <div>
                    <dt>Hardware</dt>
                    <dd>{drain.hardware_id}</dd>
                  </div>
                  <div>
                    <dt>Latitude</dt>
                    <dd>{drain.latitude}</dd>
                  </div>
                  <div>
                    <dt>Longitude</dt>
                    <dd>{drain.longitude}</dd>
                  </div>
                </dl>

                <div className={styles.actions}>
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    leftIcon={<PencilLine size={16} />}
                    onClick={() => onEdit(drain)}
                    disabled={isSaving}
                  >
                    Editar
                  </Button>

                  <Button
                    type="button"
                    variant="danger"
                    size="sm"
                    leftIcon={<Trash2 size={16} />}
                    onClick={() => onDelete(drain)}
                    disabled={isSaving}
                  >
                    Excluir
                  </Button>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
};