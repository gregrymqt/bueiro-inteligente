// Pricing.tsx
import React from 'react';
import type { PricingPlan } from '../../types';
import styles from './Pricing.module.scss';
import { Card } from '@/components/ui/Card/Card';

interface PricingProps {
  plans: PricingPlan[];
  onSelectPlan: (planId: string) => void; // Nova propriedade injetada[cite: 20]
}

export const Pricing: React.FC<PricingProps> = ({ plans, onSelectPlan }) => {
  return (
    <div className={styles.pricingGrid}>
      {plans.map((plan) => (
        <Card 
          key={plan.id} 
          title={plan.name} 
          className={`${styles.planCard} ${plan.isPopular ? styles.popular : ''}`}
        >
          {plan.isPopular && <span className={styles.badge}>Mais Popular</span>}
          <div className={styles.price}>
            R$ <span>{plan.price.toFixed(2)}</span>/mês
          </div>
          <ul className={styles.features}>
            {plan.features.map((feature, index) => (
              <li key={index}>{feature}</li>
            ))}
          </ul>
          {/* Botão agora aciona a função passando o ID do plano[cite: 20] */}
          <button 
            className={styles.planButton}
            onClick={() => onSelectPlan(plan.id)}
            aria-label={`Escolher ${plan.name}`} // Acessibilidade
          >
            Começar Agora
          </button>
        </Card>
      ))}
    </div>
  );
};