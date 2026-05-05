import React from 'react';
import { useHome } from '@/feature/home/hooks/useHome';
import { HeroSlide } from '@/feature/home/components/HeroSlide/HeroSlide';
import { HowItWorks } from '@/feature/home/components/HowItWorks/HowItWorks';
import { FeedbackList } from '@/feature/feedback/components/FeedbackList/FeedbackList';
import { Pricing } from '@/feature/plan/components/Pricing/Pricing';
import { HomeLoading } from '@/feature/home/components/HomeSkeletons/HomeSkeletons';
import styles from './Home.module.scss';
import { useNavigate } from 'react-router-dom';

const Home: React.FC = () => {
  // Removido 'reviews' do hook, pois o FeedbackList cuida disso agora[cite: 23, 24]
  const { steps, plans, loading } = useHome();
  const navigate = useNavigate();

  if (loading) return <HomeLoading />;

  const handleSelectPlan = (planId: string) => {
    // Redireciona para o Step 1 do Checkout, passando o ID do plano selecionado
    // via query string (ex: /checkout?plan=123) ou estado de navegação
    navigate(`/checkout?plan=${planId}`);
  };

  return (
    <div className={styles.homeContainer}>
      {/* Hero Section */}
      <section className={styles.heroWrapper}>
        <HeroSlide slide={{
          id: '1',
          title: "Proteja sua cidade com Inteligência",
          subtitle: "Monitoramento de bueiros em tempo real com tecnologia ESP32.",
          image_url: "/assets/hero-bg.jpg",
          section: 'hero',
          order: 1
        }} />
      </section>

      {/* Seção 2: Como Funciona */}
      <section className={styles.section} aria-label="Como Funciona">
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>Como Funciona</h2>
          <HowItWorks steps={steps} />
        </div>
      </section>

      {/* Seção 3: Planos */}
      <section className={`${styles.section} ${styles.bgAlt}`} aria-label="Planos">
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>Planos e Preços</h2>
          <Pricing plans={plans} onSelectPlan={handleSelectPlan} />
        </div>
      </section>

      {/* Seção 4: Avaliações (Agora usando a feature isolada)[cite: 23] */}
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