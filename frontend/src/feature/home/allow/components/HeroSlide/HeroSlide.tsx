// feature/home/components/HeroSlide.tsx
import { Link } from 'react-router-dom';
import type { CarouselContent } from '../../types/allow.index';
import styles from '@/pages/Home/Home.module.scss';
import { ImageResolver } from '@/core/http/ImageResolver';

interface HeroSlideProps {
  slide: CarouselContent;
  isPriority?: boolean;
}

export function HeroSlide({ slide, isPriority = false }: HeroSlideProps) {
  const mobileImage = slide.mobile_image_url || slide.desktop_image_url;

  return (
    <article className={styles.heroSlide}>
      <picture className={styles.heroMedia}>
        <source media="(max-width: 768px)" srcSet={ImageResolver.resolve(mobileImage)} />
        <img 
          className={styles.heroImage} 
          src={ImageResolver.resolve(slide.desktop_image_url)} 
          alt={slide.title}
          loading={isPriority ? "eager" : "lazy"} 
          fetchPriority={isPriority ? "high" : "auto"} 
          decoding="async" 
        />
      </picture>

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