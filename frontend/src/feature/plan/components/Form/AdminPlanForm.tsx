import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { Form } from '@/components/layout/Form';
import { useAdminPlans } from '../../hooks/useAdminPlans';
import { Plus, Trash2 } from 'lucide-react';
import styles from './AdminPlanForm.module.scss';
import type { PricingPlan } from '../../types';

type PlanFeatureField = { value: string; };

type AdminPlanFormValues = {
    name: string; amount: number; isPopular: boolean;
    frequency: number; frequencyType: string; features: PlanFeatureField[];
};

interface AdminPlanFormProps {
    initialData?: PricingPlan;
    onSuccess?: () => void;
}

export const AdminPlanForm: React.FC<AdminPlanFormProps> = ({ initialData, onSuccess }) => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { plans, addPlan, editPlan, isSubmitting, loading } = useAdminPlans();

    const planId = initialData?.id || id;
    const isEditing = Boolean(planId);
    const effectivePlan = initialData ?? plans.find(p => p.id === planId);

    const methods = useForm<AdminPlanFormValues>({
        defaultValues: {
            name: '', amount: 0, isPopular: false,
            frequency: 1, frequencyType: 'months',
            features: [{ value: '' }]
        }
    });

    useEffect(() => {
        if (isEditing && effectivePlan) {
            methods.reset({
                name: effectivePlan.name,
                amount: effectivePlan.price,
                isPopular: effectivePlan.isPopular ?? false,
                frequency: 1,
                frequencyType: 'months',
                features: effectivePlan.features?.length
                    ? effectivePlan.features.map(f => ({ value: f }))
                    : [{ value: '' }]
            });
        }
    }, [effectivePlan, methods, isEditing]);

    const { fields, append, remove } = useFieldArray({
        control: methods.control,
        name: 'features'
    });

    const onSubmit = async (data: AdminPlanFormValues) => {
        const features = data.features
            .map(f => f.value.trim())
            .filter(f => f !== '');

        if (isEditing && !planId) {
            console.error('Erro crítico: ID do plano não encontrado.');
            return;
        }

        const success = isEditing && planId
            ? await editPlan(planId, { name: data.name, amount: Number(data.amount), features, isPopular: data.isPopular })
            : await addPlan({ name: data.name, amount: Number(data.amount), features, isPopular: data.isPopular, frequency: data.frequency, frequencyType: data.frequencyType });

        if (success) {
            if (onSuccess) {
                onSuccess();
            } else {
                navigate('/admin/plans');
            }
        }
    };

    if (loading && isEditing) {
        return <p>Carregando dados do plano...</p>;
    }

    return (
        <div className={styles.formWrapper}>
            <Form methods={methods} onSubmit={onSubmit}>
                <div className={styles.formGrid}>

                    {/* Linha 1: Nome (8) + Preço (4) = 12 */}
                    <div className={styles.col8}>
                        <Form.Input
                            name="name"
                            label="Nome do Plano"
                            validation={{ required: 'O nome é obrigatório' }}
                        />
                    </div>

                    <div className={styles.col4}>
                        <Form.Input
                            name="amount"
                            label="Preço (BRL)"
                            type="number"
                            step="0.01"
                            validation={{ required: 'Defina um valor' }}
                        />
                    </div>

                    {/* Linha 2: Ciclo (6) + Checkbox (6) = 12 */}
                    <div className={styles.col6}>
                        <Form.Select
                            name="frequencyType"
                            label="Ciclo de Cobrança"
                            options={[
                                { label: 'Mensal', value: 'months' },
                                { label: 'Anual', value: 'years' }
                            ]}
                        />
                    </div>

                    <div className={`${styles.col6} ${styles.checkboxContainer}`}>
                        <Form.Checkbox
                            name="isPopular"
                            label="Destacar como Popular"
                        />
                    </div>

                    {/* Linha 3: Benefícios (12) */}
                    <div className={styles.col12}>
                        <label className={styles.label}>Benefícios do Plano</label>
                        <div className={styles.featureList}>
                            {fields.map((field, index) => (
                                <div key={field.id} className={styles.featureItem}>
                                    <Form.Input
                                        name={`features.${index}.value`}
                                        placeholder="Ex: Suporte 24h"
                                    />
                                    <button
                                        type="button"
                                        className={styles.removeBtn}
                                        onClick={() => remove(index)}
                                        aria-label="Remover benefício"
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