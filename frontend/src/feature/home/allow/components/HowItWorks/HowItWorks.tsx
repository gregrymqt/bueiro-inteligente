import React from 'react';
import type { HowItWorksStep } from '../../types';
import styles from './HowItWorks.module.scss';
// Importamos ícones do Lucide já presentes no projeto
import { Cpu, CloudUpload, BarChart3 } from 'lucide-react';

interface HowItWorksProps {
  steps: HowItWorksStep[];
}

const IconMapper: Record<string, React.ReactNode> = {
  sensor: <Cpu size={40} />,
  cloud: <CloudUpload size={40} />,
  dashboard: <BarChart3 size={40} />
};

export const HowItWorks: React.FC<HowItWorksProps> = ({ steps }) => {
  return (
    <div className={styles.stepsContainer}>
      {steps.map((step) => (
        <div key={step.id} className={styles.stepItem}>
          <div className={styles.iconWrapper}>
            {IconMapper[step.icon_name] || <Cpu size={40} />}
          </div>
          <h3>{step.title}</h3>
          <p>{step.description}</p>
        </div>
      ))}
    </div>
  );
};