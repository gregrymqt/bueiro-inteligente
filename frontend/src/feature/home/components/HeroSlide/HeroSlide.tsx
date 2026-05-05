// HeroSlide.tsx
import { Link } from 'react-router-dom';
import type { CarouselContent } from '../../types';
import styles from '@/pages/Home/Home.module.scss';

interface HeroSlideProps {
  slide: CarouselContent;
}

export function HeroSlide({ slide }: HeroSlideProps) {
  // Exemplo de como você lidaria com WebP se sua CDN/Backend suportar
  // Se não tiver WebP agora, pode usar apenas o JPG, mas a estrutura já fica pronta.
  const webpUrl = slide.image_url.replace(/\.(jpg|jpeg|png)$/, '.webp');

  return (
    <article className={styles.heroSlide}>
      <div className={styles.heroMedia}>
        <picture>
          {/* O navegador tentará carregar o WebP primeiro por ser mais leve */}
          <source srcSet={webpUrl} type="image/webp" />
          
          {/* Fallback para JPG/PNG padrão */}
          <img 
            className={styles.heroImage} 
            src={slide.image_url} 
            alt={slide.title}
            loading="eager" // Garante que não haverá lazy loading nesta imagem
            // @ts-ignore - fetchpriority é um atributo novo e pode exigir ignore dependendo da versão do @types/react
            fetchpriority="high" // Indica ao navegador que esta imagem é a prioridade número 1
            decoding="async" // Permite que o resto da página continue processando enquanto a imagem decodifica
          />
        </picture>
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