import React from 'react';
import { Carousel } from '@/components/ui/Carousel/Carousel';
import { HeroSlide } from '@/feature/home/components/HeroSlide/HeroSlide';
import { HeroSkeleton, StatsSkeleton } from '@/feature/home/components/HomeSkeletons/HomeSkeletons';
import { StatCardCarousel } from '@/feature/home/components/StatCardCarousel';
import styles from './Home.module.scss';
import { MapPin } from 'lucide-react';
import { useHomeCarousel } from '@/feature/home/hooks/useHomeCarousel';

const Home: React.FC = () => {
  const { heroSlides, statItems, loading } = useHomeCarousel();

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

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Panorama Geral do Ecossistema</h2>
        {loading ? (
          <StatsSkeleton />
        ) : (
          <StatCardCarousel items={statItems} />
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
