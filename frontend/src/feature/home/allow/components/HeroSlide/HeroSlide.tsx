// feature/home/components/HeroSlide.tsx
import { Link } from 'react-router-dom';
import type { CarouselContent } from '../../types';
import styles from '@/pages/Home/Home.module.scss';
import { ImageResolver } from '@/core/http/ImageResolver';

interface HeroSlideProps {
  slide: CarouselContent;
}

export function HeroSlide({ slide }: HeroSlideProps) {
  const resolvedImageUrl = ImageResolver.resolve(slide.image_url);

  return (
    <article className={styles.heroSlide}>
      <div className={styles.heroMedia}>
        {/* Removemos o <picture> e a lógica de .webp para carregar apenas o arquivo real */}
        <img 
          className={styles.heroImage} 
          src={resolvedImageUrl} 
          alt={slide.title}
          loading="eager" 
          fetchPriority="high" 
          decoding="async" 
        />
      </div>

      <div className={styles.heroContent}>
        <h2 className={styles.heroTitle}>{slide.title}</h2>
        {slide.subtitle && <p className={styles.heroSubtitle}>{slide.subtitle}</p>}
        {slide.action_url && (
          <Link to={slide.action_url} className={styles.heroButton}>
            Saiba Mais
          </Link>
        )}
      </div>
    </article>
  );
}