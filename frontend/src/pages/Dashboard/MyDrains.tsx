import { useState } from 'react';
import { Plus } from 'lucide-react';
import { AlertService } from '@/core/alert/AlertService';
import { Button } from '@/components/ui/Button/Button';
import { DrainForm } from '@/feature/drain/components/DrainForm';
import { DrainList } from '@/feature/drain/components/DrainList';
import { useDrainAdmin } from '@/feature/drain/hooks/useDrains';
import type { Drain, DrainCreatePayload } from '@/feature/drain/types';
import styles from './MyDrains.module.scss';

export const MyDrains = () => {
  // Use mock mode for hooks if permissions fail, or we just pass true for useMock if we modify the hook.
  // For now we will assume the hook uses mock appropriately or we'll ensure the hook has useMock flag.
  const { drains, loading, isSaving, createDrain, updateDrain, deleteDrain } = useDrainAdmin();
  const [editingDrain, setEditingDrain] = useState<Drain | undefined>(undefined);
  const [formVersion, setFormVersion] = useState(0);

  const resetForm = () => {
    setEditingDrain(undefined);
    setFormVersion((currentVersion) => currentVersion + 1);
  };

  const handleCreateOrUpdate = async (payload: DrainCreatePayload): Promise<void> => {
    const success = editingDrain
      ? await updateDrain(editingDrain.id, payload)
      : await createDrain(payload);

    if (success) {
      resetForm();
    }
  };

  const handleDelete = async (drain: Drain): Promise<void> => {
    await AlertService.confirm({
      title: 'Confirmar exclusão',
      text: `Tem certeza que deseja excluir o bueiro "${drain.name}"?`,
      onConfirm: async () => {
        const success = await deleteDrain(drain.id);

        if (success && editingDrain?.id === drain.id) {
          resetForm();
        }
      },
    });
  };

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Área do Usuário</p>
          <h1 className={styles.title}>Meus Bueiros</h1>
          <p className={styles.subtitle}>
            Gerencie seus bueiros monitorados.
          </p>
        </div>

        <Button type="button" onClick={resetForm} leftIcon={<Plus size={16} />}>
          Novo bueiro
        </Button>
      </header>

      <div className={styles.grid}>
        <DrainList
          drains={drains}
          loading={loading}
          isSaving={isSaving}
          selectedDrainId={editingDrain?.id}
          onEdit={setEditingDrain}
          onDelete={handleDelete}
        />

        <DrainForm
          key={formVersion}
          initialData={editingDrain}
          onSubmit={handleCreateOrUpdate}
          onCancel={editingDrain ? resetForm : undefined}
          isLoading={isSaving}
        />
      </div>
    </div>
  );
};