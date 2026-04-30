import React from 'react';
import { Cpu, Cloud, LayoutDashboard } from 'lucide-react';
import styles from './HowItWorks.module.scss';
import { Card } from '@/components/ui/Card/Card';

export const HowItWorks: React.FC = () => {
  const steps = [
    {
      id: 'step-1',
      title: 'Captura (Sensor)',
      description: 'Sensores ESP32 medem o nível de água e acúmulo de lixo em tempo real.',
      icon: <Cpu size={32} className={styles.icon} />,
    },
    {
      id: 'step-2',
      title: 'Processamento (Nuvem)',
      description: 'Os dados são enviados e analisados na nuvem para identificar riscos de alagamento.',
      icon: <Cloud size={32} className={styles.icon} />,
    },
    {
      id: 'step-3',
      title: 'Dashboard (Monitoramento)',
      description: 'Equipes visualizam os alertas no painel e tomam decisões ágeis.',
      icon: <LayoutDashboard size={32} className={styles.icon} />,
    },
  ];

  return (
    <div className={styles.container}>
      {steps.map((step, index) => (
        <React.Fragment key={step.id}>
          <Card className={styles.stepCard}>
            <div className={styles.iconContainer}>{step.icon}</div>
            <h3 className={styles.title}>{step.title}</h3>
            <p className={styles.description}>{step.description}</p>
          </Card>
          {index < steps.length - 1 && (
            <div className={styles.connector} aria-hidden="true" />
          )}
        </React.Fragment>
      ))}
    </div>
  );
};
