import React, { type ReactNode } from 'react';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Navigation, Pagination, Autoplay } from 'swiper/modules';
import type { SwiperOptions } from 'swiper/types';

// Importação dos estilos obrigatórios do Swiper
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';

import './Carousel.scss';

export interface CarouselProps {
  /**
   * Array de componentes ou elementos HTML que serão os slides.
   */
  slides: ReactNode[];
  /**
   * Número de slides visíveis ao mesmo tempo.
   */
  slidesPerView?: number | 'auto';
  /**
   * Espaçamento entre os slides (em px).
   */
  spaceBetween?: number;
  /**
   * Ativar as setas de navegação (próximo/anterior).
   */
  navigation?: boolean;
  /**
   * Ativar paginação (as "bolinhas" indicadoras embaixo).
   */
  pagination?: boolean | SwiperOptions['pagination'];
  /**
   * Fazer o carrossel repetir em loop.
   */
  loop?: boolean;
  /**
   * Ativar autoplay. Pode ser um boolean ou um objeto de configuração.
   */
  autoplay?: boolean | SwiperOptions['autoplay'];
  /**
   * Classes extras para o wrapper principal.
   */
  className?: string;
  /**
   * Configuração de responsividade. Chave é o breakpoint (em px), valor é a configuração.
   */
  breakpoints?: SwiperOptions['breakpoints'];
}

export const Carousel: React.FC<CarouselProps> = ({
  slides,
  slidesPerView = 1,
  spaceBetween = 20,
  navigation = true,
  pagination = { clickable: true },
  loop = true,
  autoplay = { delay: 3000, disableOnInteraction: false },
  className = '',
  breakpoints = {
    768: {
      slidesPerView: 3,
    },
  },
}) => {
  // Configurando os módulos ativados condicionalmente
  const modules = [];
  if (navigation) modules.push(Navigation);
  if (pagination) modules.push(Pagination);
  if (autoplay) modules.push(Autoplay);

  return (
    <div className={`generic-carousel ${className}`}>
      <Swiper
        modules={modules}
        slidesPerView={slidesPerView}
        spaceBetween={spaceBetween}
        navigation={navigation}
        pagination={pagination}
        loop={loop}
        autoplay={autoplay}
        breakpoints={breakpoints}
        className="generic-swiper"
      >
        {slides.map((slide, index) => (
          // Usamos o index como key fallback, 
          // caso o conteúdo não possua um identificador próprio
          <SwiperSlide key={`slide-${index}`}>{slide}</SwiperSlide>
        ))}
      </Swiper>
    </div>
  );
};
