import React from 'react';
import { Carousel } from '@/components/ui/Carousel/Carousel';
import { HeroSlide } from '@/feature/home/components/HeroSlide/HeroSlide';
import {
  HeroSkeleton,
  StatsSkeleton,
  PricingSkeleton,
  ReviewsSkeleton
} from '@/feature/home/components/HomeSkeletons/HomeSkeletons';
import { StatCardCarousel } from '@/feature/home/components/StatCardCarousel';
import { HowItWorks } from '@/feature/home/components/HowItWorks/HowItWorks';
import { Pricing } from '@/feature/home/components/Pricing/Pricing';
import { Reviews } from '@/feature/home/components/Reviews/Reviews';
import styles from './Home.module.scss';
import { useHomeCarousel } from '@/feature/home/hooks/useHomeCarousel';

const Home: React.FC = () => {
  const { heroSlides, statItems, plans, reviews, loading } = useHomeCarousel();

  return (
    <div className={styles.homeContainer}>
      {/* Seção 1: Banner Hero */}
      <section className={styles.sectionHero} aria-label="Destaques">
        {loading ? (
          <HeroSkeleton />
        ) : (
          <Carousel
            slides={heroSlides.map((slide) => (
              <HeroSlide key={slide.id} slide={slide} />
            ))}
            pagination={true}
            autoplay={{ delay: 5000, disableOnInteraction: false }}
          />
        )}
      </section>

      {/* Seção 2: Como Funciona */}
      <section className={styles.section} aria-label="Como Funciona">
        <h2 className={styles.sectionTitle}>Como Funciona</h2>
        <HowItWorks />
      </section>

      {/* Seção Estatísticas Extras (opcional, mantida para contexto e coerência com a refatoração, mas renomeada para mostrar o Panorama) */}
      <section className={styles.section} aria-label="Panorama Geral do Ecossistema">
        <h2 className={styles.sectionTitle}>Panorama Geral do Ecossistema</h2>
        {loading ? (
          <StatsSkeleton />
        ) : (
          <StatCardCarousel items={statItems} />
        )}
      </section>

      {/* Seção 3: Planos de Serviço */}
      <section className={styles.section} aria-label="Planos de Serviço">
        <h2 className={styles.sectionTitle}>Planos de Serviço</h2>
        {loading ? (
          <PricingSkeleton />
        ) : (
          <Pricing plans={plans} />
        )}
      </section>

      {/* Seção 4: Avaliações */}
      <section className={styles.section} aria-label="Avaliações">
        <h2 className={styles.sectionTitle}>O que dizem sobre nós</h2>
        {loading ? (
          <ReviewsSkeleton />
        ) : (
          <Reviews reviews={reviews} />
        )}
      </section>

      {/* Seção 5: Footer (Nota: Footer já está no MainLayout,
          então sua inclusão aqui não é necessária para evitar duplicação.
          A instrução diz "Integre o componente de layout Footer", mas a MainLayout
          já o renderiza. Para seguir estritamente o layout e não duplicar,
          vamos apenas confirmar a responsabilidade.
      */}
    </div>
  );
};

export default Home;