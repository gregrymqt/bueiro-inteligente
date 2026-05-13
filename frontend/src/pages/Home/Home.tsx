// src/pages/Home/Home.tsx
import React from 'react';
import { useHome } from '@/feature/home/allow/hooks/useHome';
import { HeroSlide } from '@/feature/home/allow/components/HeroSlide/HeroSlide';
import { HowItWorks } from '@/feature/home/allow/components/HowItWorks/HowItWorks';
import { SmartAppBanner } from '@/feature/home/allow/components/SmartAppBanner/SmartAppBanner';
import { FeedbackList } from '@/feature/feedback/components/FeedbackList/FeedbackList';
import { Pricing } from '@/feature/plan/components/Pricing/Pricing';
import { HomeLoading } from '@/feature/home/allow/components/HomeSkeletons/HomeSkeletons';
import styles from './Home.module.scss';
import { useNavigate } from 'react-router-dom';

const Home: React.FC = () => {
  // Extraímos corretamente os dados do hook
  const { carousels, stats, plans, loading } = useHome();
  const navigate = useNavigate();

  const handleSelectPlan = (planId: string) => {
    navigate(`/checkout?plan=${planId}`);
  };

  const smartAppBanner = <SmartAppBanner />;

  if (loading) return <HomeLoading banner={smartAppBanner} />;

  return (
    <div className={styles.homeContainer}>
      {smartAppBanner}

      {/* Hero Section - Agora usando o primeiro slide vindo do Banco de Dados */}
      <section className={styles.heroWrapper}>
        {carousels.length > 0 ? (
          <HeroSlide slide={carousels[0]} />
        ) : (
          // Fallback caso não existam slides no banco
          <HeroSlide slide={{
            id: '1',
            title: "Proteja sua cidade com Inteligência",
            subtitle: "Monitoramento de bueiros em tempo real com tecnologia ESP32.",
            image_url: "/assets/hero-bg.jpg",
            section: 'hero',
            order: 1
          }} />
        )}
      </section>

      {/* Seção 2: Como Funciona (Usando os dados de 'stats' do backend) */}
      <section className={styles.section} aria-label="Como Funciona">
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>Como Funciona</h2>
          {/* Alterado de 'steps' para 'stats' que é o que o hook retorna agora */}
          <HowItWorks steps={stats} />
        </div>
      </section>

      {/* Seção 3: Planos */}
      <section className={`${styles.section} ${styles.bgAlt}`} aria-label="Planos">
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>Planos e Preços</h2>
          <Pricing plans={plans} onSelectPlan={handleSelectPlan} />
        </div>
      </section>

      {/* Seção 4: Avaliações */}
      <section className={styles.section} aria-label="Avaliações">
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>Depoimentos</h2>
          <FeedbackList />
        </div>
      </section>
    </div>
  );
};

export default Home;