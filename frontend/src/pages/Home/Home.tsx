import React from 'react';
import { Carousel } from '@/components/ui/Carousel/Carousel';
import { HeroSlide } from '@/feature/home/components/HeroSlide/HeroSlide';
<<<<<<< HEAD
import {
  HeroSkeleton,
  HowItWorksSkeleton,
  PricingSkeleton,
  ReviewsSkeleton
} from '@/feature/home/components/HomeSkeletons/HomeSkeletons';
import { HowItWorks } from '@/feature/home/components/HowItWorks/HowItWorks';
import { Pricing } from '@/feature/home/components/Pricing/Pricing';
import { Reviews } from '@/feature/home/components/Reviews/Reviews';
=======
import { HeroSkeleton, StatsSkeleton } from '@/feature/home/components/HomeSkeletons/HomeSkeletons';
import { StatCardCarousel } from '@/feature/home/components/StatCardCarousel';
>>>>>>> master
import styles from './Home.module.scss';
import { MapPin } from 'lucide-react';
import { useHomeCarousel } from '@/feature/home/hooks/useHomeCarousel';

const Home: React.FC = () => {
<<<<<<< HEAD
  const { heroSlides, plans, reviews, loading } = useHomeCarousel();
=======
  const { heroSlides, statItems, loading } = useHomeCarousel();
>>>>>>> master

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

<<<<<<< HEAD
      {/* Seção 2: Como Funciona */}
      <section className={styles.section} aria-label="Como Funciona">
        <h2 className={styles.sectionTitle}>Como Funciona</h2>
=======
      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Panorama Geral do Ecossistema</h2>
>>>>>>> master
        {loading ? (
          <HowItWorksSkeleton />
        ) : (
          <HowItWorks />
        )}
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
