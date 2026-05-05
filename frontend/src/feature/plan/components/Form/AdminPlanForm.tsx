import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom'; // 1. Hooks do Router
import { useForm, useFieldArray } from 'react-hook-form';
import { Form } from '@/components/layout/Form';
import { useAdminPlans } from '../../hooks/useAdminPlans';
import { Plus, Trash2 } from 'lucide-react';
import styles from './AdminPlanForm.module.scss';

type PlanFeatureField = { value: string; };

type AdminPlanFormValues = {
    name: string; amount: number; isPopular: boolean;
    frequency: number; frequencyType: string; features: PlanFeatureField[];
};

// 2. Não precisa mais de props (initialData, onSuccess)
export const AdminPlanForm: React.FC = () => {
    const { id } = useParams<{ id: string }>(); // Pega o ID da URL se estiver editando
    const navigate = useNavigate();

    // Extraímos 'plans' e 'loading' para poder buscar os dados da edição
    const { plans, addPlan, editPlan, isSubmitting, loading } = useAdminPlans();
    const isEditing = Boolean(id);

    const methods = useForm<AdminPlanFormValues>({
        defaultValues: {
            name: '', amount: 0, isPopular: false,
            frequency: 1, frequencyType: 'months',
            features: [{ value: '' }]
        }
    });

    // 3. Preenche o formulário assim que a lista de planos carregar
    useEffect(() => {
        if (isEditing && plans.length > 0) {
            const planToEdit = plans.find(p => p.id === id);
            if (planToEdit) {
                methods.reset({
                    name: planToEdit.name,
                    amount: planToEdit.price,
                    isPopular: planToEdit.isPopular ?? false,
                    frequency: 1, // Valores fixos assumidos ou vindos do backend
                    frequencyType: 'months',
                    features: planToEdit.features?.length
                        ? planToEdit.features.map(f => ({ value: f }))
                        : [{ value: '' }]
                });
            }
        }
    }, [id, plans, methods, isEditing]);

    const { fields, append, remove } = useFieldArray({
        control: methods.control,
        name: 'features'
    });

    const onSubmit = async (data: AdminPlanFormValues) => {
        const features = data.features
            .map(f => f.value.trim())
            .filter(f => f !== '');

        const success = isEditing && id
            ? await editPlan(id, { name: data.name, amount: Number(data.amount), features, isPopular: data.isPopular })
            : await addPlan({ name: data.name, amount: Number(data.amount), features, isPopular: data.isPopular, frequency: data.frequency, frequencyType: data.frequencyType });

        if (success) {
            navigate('/admin/plans'); // 4. Voltar para a lista em caso de sucesso
        }
    };

    if (loading && isEditing) {
        return <p>Carregando dados do plano...</p>;
    }

    return (
        <div className={styles.formWrapper}>
            <Form methods={methods} onSubmit={onSubmit}>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(12, 1fr)', gap: '1rem' }}>

                    <Form.Input
                        name="name"
                        label="Nome do Plano"
                        colSpan={6}
                        validation={{ required: 'O nome é obrigatório' }}
                    />

                    <Form.Input
                        name="amount"
                        label="Preço (BRL)"
                        type="number"
                        colSpan={4}
                        step="0.01"
                        validation={{ required: 'Defina um valor' }}
                    />

                    <Form.Select
                        name="frequencyType"
                        label="Ciclo de Cobrança"
                        colSpan={6}
                        options={[
                            { label: 'Mensal', value: 'months' },
                            { label: 'Anual', value: 'years' }
                        ]}
                    />

                    <Form.Checkbox
                        name="isPopular"
                        label="Destacar como Popular"
                        colSpan={6}
                    />

                    {/* Gerenciamento de Benefícios (Features) */}
                    <div style={{ gridColumn: 'span 12' }}>
                        <label className={styles.label}>Benefícios do Plano</label>
                        <div className={styles.featureList}>
                            {fields.map((field, index) => (
                                <div key={field.id} className={styles.featureItem}>
                                    <Form.Input
                                        name={`features.${index}.value`}
                                        placeholder="Ex: Suporte 24h"
                                        colSpan={12}
                                    />
                                    <button
                                        type="button"
                                        className={styles.removeBtn}
                                        onClick={() => remove(index)}
                                    >
                                        <Trash2 size={18} />
                                    </button>
                                </div>
                            ))}
                        </div>
                        <button
                            type="button"
                            className={styles.addFeatureBtn}
                            onClick={() => append({ value: '' })}
                        >
                            <Plus size={16} style={{ marginRight: '8px' }} /> Adicionar Benefício
                        </button>
                    </div>
                </div>

                <Form.Actions>
                    <Form.Submit isLoading={isSubmitting}>
                        {isEditing ? 'Salvar Alterações' : 'Criar Plano no Mercado Pago'}
                    </Form.Submit>
                </Form.Actions>
            </Form>
        </div>
    );
};