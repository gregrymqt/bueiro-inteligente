import React from 'react';
import { Carousel } from '@/components/ui/Carousel/Carousel';
import { HeroSlide } from '@/feature/home/components/HeroSlide/HeroSlide';
import {
  HeroSkeleton,
  HowItWorksSkeleton,
  PricingSkeleton,
  ReviewsSkeleton
} from '@/feature/home/components/HomeSkeletons/HomeSkeletons';
import { HowItWorks } from '@/feature/home/components/HowItWorks/HowItWorks';
import { Pricing } from '@/feature/home/components/Pricing/Pricing';
import { Reviews } from '@/feature/home/components/Reviews/Reviews';
import styles from './Home.module.scss';
import { MapPin } from 'lucide-react';
import { useHomeCarousel } from '@/feature/home/hooks/useHomeCarousel';

const Home: React.FC = () => {
  const { heroSlides, plans, reviews, loading } = useHomeCarousel();

  return (
    <div className={styles.homeContainer}>
      <section className={styles.section}>
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
        {loading ? <HowItWorksSkeleton /> : <HowItWorks />}
      </section>

      {/* CORRIGIDO: Seção 3: Planos (Estava importada mas não usada) */}
      <section className={styles.section} aria-label="Planos de Serviço">
        {loading ? <PricingSkeleton /> : <Pricing plans={plans || []} />}
      </section>

      {/* CORRIGIDO: Seção 4: Avaliações (Estava importada mas não usada) */}
      <section className={styles.section} aria-label="Avaliações de Clientes">
        {loading ? <ReviewsSkeleton /> : <Reviews reviews={reviews || []} />}
      </section>

      <section className={styles.section}>
        <div className={styles.mapMock}>
          <MapPin size={32} color="#ffffff" />
          <span>Mapa Interativo (Em Breve)</span>
        </div>
      </section>
    </div>
  );
};

export default Home;