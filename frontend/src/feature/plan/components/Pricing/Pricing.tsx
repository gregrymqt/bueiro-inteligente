import React from 'react';
import type { PricingPlan } from '../../types';
import styles from './Pricing.module.scss';
import { Card } from '@/components/ui/Card/Card';

interface PricingProps {
  plans: PricingPlan[];
}

export const Pricing: React.FC<PricingProps> = ({ plans }) => {
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
          <button className={styles.planButton}>Começar Agora</button>
        </Card>
      ))}
    </div>
  );
};