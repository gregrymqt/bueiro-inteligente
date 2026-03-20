import React from 'react';
import { useHomeCarousel } from '../feature/home/hooks/useHomeCarousel';
import { GenericCarousel } from '@/components/common/GenericCarousel';
import { StatCardCarousel } from '../feature/home/components/StatCardCarousel'; // Componente que vamos criar abaixo
import styles from './Home.module.scss';
import { Target, AlertTriangle, Zap, MapPin } from 'lucide-react'; // Ícones de exemplo

const Home: React.FC = () => {
  // Hook alimentando o Hero Carousel
  const { items: heroSlides, loading: heroLoading } = useHomeCarousel('hero');
  
  // Hook alimentando os Stat Cards. Aqui 'loading' e 'data' vem do hook.
  const { items: statItems, loading: statLoading } = useHomeCarousel('stats');

  return (
    <div className={styles.homeContainer}>
      {/* Seção 1: Hero Carousel (Slides Principais) */}
      <section className={`${styles.section} ${styles.sectionHero}`}>
        {heroLoading ? (
          <div className={styles.skeleton}>Carregando destaques...</div>
        ) : (
          <GenericCarousel 
            items={heroSlides} 
            showPagination 
            autoPlay 
          />
        )}
      </section>

      {/* Seção 2: Stat Cards Carousel (A que você pediu) */}
      <section className={`${styles.section} ${styles.sectionStats}`}>
        <h2 className={styles.sectionTitle}>Panorama Geral do Ecossistema</h2>
        {statLoading ? (
          <div className={styles.skeletonStats}>Carregando métricas...</div>
        ) : (
          <StatCardCarousel items={statItems} />
        )}
      </section>

      {/* Seção 3: Mapa ou Outro Componente (UI/UX) */}
      <section className={`${styles.section} ${styles.sectionMap}`}>
        <div className={styles.mapMock}>
          <MapPin size={32} color="#ffffff" />
          <span>Mapa Interativo (Em Breve)</span>
        </div>
      </section>
    </div>
  );
};

export default Home;