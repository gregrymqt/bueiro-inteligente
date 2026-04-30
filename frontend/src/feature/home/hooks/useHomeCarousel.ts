import { useState, useCallback, useEffect } from 'react';
import { HomeService } from '../services/HomeService';
import { AlertService } from '@/core/alert/AlertService';
import type { CarouselContent, StatCardContent, PricingPlan, UserReview } from '../types';

const USE_HOME_CAROUSEL_MOCK = false;

interface UseHomeCarouselResult {
  heroSlides: CarouselContent[];
  statItems: StatCardContent[];
  plans: PricingPlan[];
  reviews: UserReview[];
  loading: boolean;
  isMockMode: boolean;
}

export function useHomeCarousel(): UseHomeCarouselResult {
  const [heroSlides, setHeroSlides] = useState<CarouselContent[]>([]);
  const [statItems, setStatItems] = useState<StatCardContent[]>([]);
  const [plans, setPlans] = useState<PricingPlan[]>([]);
  const [reviews, setReviews] = useState<UserReview[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchData = useCallback(async () => {
    setLoading(true);

    try {
      const data = await HomeService.getHomeData(USE_HOME_CAROUSEL_MOCK);
      const filteredHeroSlides = [...data.carousels]
        .filter((carousel) => carousel.section === 'hero')
        .sort((a, b) => a.order - b.order);
      const orderedStatItems = [...data.stats].sort((a, b) => a.order - b.order);
      const orderedPlans = [...(data.plans || [])].sort((a, b) => a.order - b.order);
      const orderedReviews = [...(data.reviews || [])].sort((a, b) => a.order - b.order);

      setHeroSlides(filteredHeroSlides);
      setStatItems(orderedStatItems);
      setPlans(orderedPlans);
      setReviews(orderedReviews);
    } catch {
      AlertService.error('Erro', 'Erro ao carregar dados da Home.');
      setHeroSlides([]);
      setStatItems([]);
      setPlans([]);
      setReviews([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return { heroSlides, statItems, plans, reviews, loading, isMockMode: USE_HOME_CAROUSEL_MOCK };
}