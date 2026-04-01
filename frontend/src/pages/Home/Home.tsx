import React from 'react';
import { Carousel } from '@/components/ui/Carousel/Carousel';
import { StatCardCarousel } from '@/feature/home/components/StatCardCarousel';
import styles from './Home.module.scss';
import { MapPin } from 'lucide-react';
import { type StatCardContent } from '@/feature/home/types';
import { useHomeCarousel } from '@/feature/home/hooks/useHomeCarousel';

const Home: React.FC = () => {
  const { items: heroSlides, loading: heroLoading } = useHomeCarousel('hero');
  const { items: rawStatItems, loading: statLoading } = useHomeCarousel('stats');

  const statItems: StatCardContent[] = rawStatItems.map((item: any) => ({
    id: item.id,
    title: item.title,
    value: item.subtitle || "0",
    description: item.title || "Visão Geral",
    icon_name: 'Target',
    color: 'warning',
    order: item.order
  }));

  return (
    <div className={styles.homeContainer}>
      <section className={` `}>
        {heroLoading ? (
          <div className={styles.skeleton}>Carregando destaques...</div>
        ) : (
          <Carousel
            slides={heroSlides.map((slide: any) => (
              <div key={slide.id} className={styles.heroSlide} style={{ backgroundImage: "url()", backgroundSize: 'cover', backgroundPosition: 'center', minHeight: '400px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <div className={styles.heroContent} style={{ backgroundColor: 'rgba(0,0,0,0.6)', padding: '2rem', borderRadius: '8px', color: '#fff', textAlign: 'center', maxWidth: '80%' }}>
                  <h2 style={{ fontSize: '2rem', margin: '0 0 1rem 0' }}>{slide.title}</h2>
                  {slide.subtitle && <p style={{ fontSize: '1.2rem', margin: '0 0 1.5rem 0' }}>{slide.subtitle}</p>}
                  {slide.actionUrl && (
                    <a href={slide.actionUrl} style={{ display: 'inline-block', padding: '0.8rem 1.5rem', backgroundColor: '#0056b3', color: '#fff', textDecoration: 'none', borderRadius: '4px', fontWeight: 'bold' }}>
                      Saiba Mais
                    </a>
                  )}
                </div>
              </div>
            ))}
            pagination={true}
            autoplay={{ delay: 5000, disableOnInteraction: false }}
          />
        )}
      </section>

      <section className={` `}>
        <h2 className={styles.sectionTitle}>Panorama Geral do Ecossistema</h2>
        {statLoading ? (
          <div className={styles.skeletonStats}>Carregando métricas...</div>
        ) : (
          <StatCardCarousel items={statItems} />
        )}
      </section>

      <section className={` `}>
        <div className={styles.mapMock}>
          <MapPin size={32} color="#ffffff" />
          <span>Mapa Interativo (Em Breve)</span>
        </div>
      </section>
    </div>
  );
};

export default Home;

