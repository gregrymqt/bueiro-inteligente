import React from 'react';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Pagination } from 'swiper/modules';
import type { StatCardContent } from '../types';
import { StatCard } from './StatCard';

import 'swiper/css';
import 'swiper/css/pagination';
import './StatCardCarousel.scss';

interface StatCardCarouselProps {
  items: StatCardContent[];
}

export const StatCardCarousel: React.FC<StatCardCarouselProps> = ({ items }) => {
  return (
    <div className="stat-card-carousel">
      <Swiper
        modules={[Pagination]}
        pagination={{ clickable: true }}
        spaceBetween={16} // $spacing-md
        slidesPerView={1} // Base Mobile-First: 1 slide por vez
        breakpoints={{
          // No Desktop (>= 768px), mostramos 3 slides
          768: {
            slidesPerView: 3,
            spaceBetween: 24, // $spacing-lg
          },
        }}
      >
        {items.map((item) => (
          <SwiperSlide key={item.id}>
            <StatCard 
              iconName={item.icon_name}
              title={item.title}
              value={item.value}
              description={item.description}
              color={item.color as "primary" | "success" | "warning" | "danger" | undefined}
            />
          </SwiperSlide>
        ))}
      </Swiper>
    </div>
  );
};