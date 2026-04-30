import React from 'react';
import { CheckCircle2 } from 'lucide-react';
import styles from './Pricing.module.scss';
import { Card } from '@/components/ui/Card/Card';
import type { PricingPlan } from '../../types';

interface PricingProps {
  plans: PricingPlan[];
}

export const Pricing: React.FC<PricingProps> = ({ plans }) => {
  return (
    <div className={styles.grid}>
      {plans.map((plan) => (
        <Card
          key={plan.id}
          className={`${styles.planCard} ${plan.isPopular ? styles.popular : ''}`}
        >
          {plan.isPopular && <div className={styles.popularBadge}>Mais Escolhido</div>}
          <div className={styles.header}>
            <h3 className={styles.name}>{plan.name}</h3>
            <div className={styles.price}>{plan.price}</div>
            <p className={styles.description}>{plan.description}</p>
          </div>
          <div className={styles.features}>
            {plan.features.map((feature, index) => (
              <div key={index} className={styles.featureItem}>
                <CheckCircle2 size={20} className={styles.featureIcon} />
                <span>{feature}</span>
              </div>
            ))}
          </div>
          <button className={styles.actionButton}>Assinar Agora</button>
        </Card>
      ))}
    </div>
  );
};
