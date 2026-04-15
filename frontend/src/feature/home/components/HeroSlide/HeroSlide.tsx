import { Link } from 'react-router-dom';
import type { CarouselContent } from '../../types';
import styles from '@/pages/Home/Home.module.scss';

interface HeroSlideProps {
  slide: CarouselContent;
}

export function HeroSlide({ slide }: HeroSlideProps) {
  return (
    <article className={styles.heroSlide}>
      <div className={styles.heroMedia}>
        <img className={styles.heroImage} src={slide.image_url} alt={slide.title} />
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