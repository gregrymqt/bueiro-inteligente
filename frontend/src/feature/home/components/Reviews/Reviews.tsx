import React from 'react';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Autoplay, Pagination } from 'swiper/modules';
import { Star } from 'lucide-react';
import styles from './Reviews.module.scss';
import { Card } from '@/components/ui/Card/Card';
import type { UserReview } from '../../types';

import 'swiper/css';
import 'swiper/css/pagination';

interface ReviewsProps {
  reviews: UserReview[];
}

export const Reviews: React.FC<ReviewsProps> = ({ reviews }) => {
  return (
    <div className={styles.container}>
      <Swiper
        modules={[Autoplay, Pagination]}
        spaceBetween={24}
        slidesPerView={1}
        pagination={{ clickable: true }}
        autoplay={{ delay: 5000, disableOnInteraction: false }}
        breakpoints={{
          768: {
            slidesPerView: 2,
          },
          1024: {
            slidesPerView: 3,
          },
        }}
        className={styles.swiper}
      >
        {reviews.map((review) => (
          <SwiperSlide key={review.id} className={styles.slide}>
            <Card className={styles.reviewCard}>
              <div className={styles.rating}>
                {Array.from({ length: 5 }).map((_, index) => (
                  <Star
                    key={index}
                    size={16}
                    className={index < review.rating ? styles.starFilled : styles.starEmpty}
                  />
                ))}
              </div>
              <p className={styles.content}>"{review.content}"</p>
              <div className={styles.author}>
                {review.avatarUrl ? (
                  <img src={review.avatarUrl} alt={review.authorName} className={styles.avatar} />
                ) : (
                  <div className={styles.avatarPlaceholder}>
                    {review.authorName.charAt(0).toUpperCase()}
                  </div>
                )}
                <div className={styles.authorInfo}>
                  <strong className={styles.authorName}>{review.authorName}</strong>
                  <span className={styles.authorRole}>{review.authorRole}</span>
                </div>
              </div>
            </Card>
          </SwiperSlide>
        ))}
      </Swiper>
    </div>
  );
};
