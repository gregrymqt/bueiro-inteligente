import React from 'react';
// 1. Altere a importação aqui de HowItWorksStep para StatCardContent
import type { StatCardContent } from '../../types/allow.index'; 
import styles from './HowItWorks.module.scss';
import { Cpu, CloudUpload, BarChart3 } from 'lucide-react';

// 2. Altere o tipo da prop aqui para receber a lista de estatísticas do banco
interface HowItWorksProps {
  steps: StatCardContent[]; 
}

const IconMapper: Record<string, React.ReactNode> = {
  sensor: <Cpu size={40} />,
  cloud: <CloudUpload size={40} />,
  dashboard: <BarChart3 size={40} />
};

export const HowItWorks: React.FC<HowItWorksProps> = ({ steps = [] }) => {
  if (!steps || steps.length === 0) return null; 
  
  return (
    <div className={styles.stepsContainer}>
      {steps.map((step) => (
        <div key={step.id} className={styles.stepItem}>
          <div className={styles.iconWrapper}>
            {IconMapper[step.icon_name] || <Cpu size={40} />}
          </div>
          {/* ✨ Agora o TypeScript reconhecerá o .value perfeitamente! */}
          <h2 className="stat-value">{step.value}</h2>
          <h3>{step.title}</h3>
          <p>{step.description}</p>
        </div>
      ))}
    </div>
  );
};