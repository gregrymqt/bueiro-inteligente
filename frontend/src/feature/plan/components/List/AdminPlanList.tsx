import React from 'react';
import { useNavigate } from 'react-router-dom'; // 1. Importado
import { useAdminPlans } from '../../hooks/useAdminPlans';
import { AlertService } from '@/core/alert/AlertService';
import { Pencil, Power, PowerOff, RefreshCw } from 'lucide-react';
import type { PricingPlan } from '../../types';
import styles from './AdminPlanList.module.scss';
import { GenericTable } from '@/components/ui/Table/GenericTable';

// 2. Interface limpa, sem props de navegação
export const AdminPlanList: React.FC = () => {
  const { plans, loading, toggleStatus, refresh } = useAdminPlans();
  const navigate = useNavigate(); // 3. Hook de navegação

  const handleToggleStatus = async (plan: PricingPlan) => {
    const isActivating = plan.status === 'inactive';
    await AlertService.confirm({
      title: 'Confirmar Operação',
      text: `Tem certeza que deseja ${isActivating ? 'ativar' : 'inativar'} o plano "${plan.name}"?`,
      onConfirm: async () => {
        await toggleStatus(plan.id, plan.status);
      }
    });
  };

  const columns = [
    { key: 'name', label: 'Plano' },
    { key: 'price', label: 'Preço', render: (price: number) => `R$ ${price.toFixed(2)}` },
    {
      key: 'status', label: 'Status',
      render: (status: string) => (
        <span className={`${styles.badge} ${styles[status]}`}>
          {status === 'active' ? 'Ativo' : 'Inativo'}
        </span>
      )
    },
    {
      key: 'actions', label: 'Ações',
      render: (_value: unknown, plan: PricingPlan) => (
        <div className={styles.actionButtons}>
          <button
            // 4. Navegação direta pela URL
            onClick={() => navigate(`/admin/plans/edit/${plan.id}`)}
            className={styles.editBtn}
            title="Editar Plano"
          >
            <Pencil size={18} />
          </button>
          <button
            onClick={() => handleToggleStatus(plan)}
            className={plan.status === 'active' ? styles.deactivateBtn : styles.activateBtn}
            title={plan.status === 'active' ? "Inativar" : "Ativar"}
          >
            {plan.status === 'active' ? <PowerOff size={18} /> : <Power size={18} />}
          </button>
        </div>
      )
    }
  ];

  return (
    <div className={styles.listContainer}>
      <header className={styles.header}>
        <h3>Planos Cadastrados</h3>
        <button onClick={refresh} className={styles.refreshBtn}>
          <RefreshCw size={16} /> Atualizar
        </button>
      </header>
      <GenericTable data={plans} columns={columns} isLoading={loading} />
    </div>
  );
};