import { useState, useEffect, useCallback } from 'react';
import { HomeService } from '../services/HomeService';
import type { CarouselContent, CarouselSection } from '../types';

export const useHomeCarousel = (section: CarouselSection) => {
  const [items, setItems] = useState<CarouselContent[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const loadItems = useCallback(async () => {
    try {
      setLoading(true);
      const data = await HomeService.getCarouselItems(section);
      setItems(data.sort((a, b) => a.order - b.order)); // Garante a ordem definida no back
    } catch (err) {
      setError('Não foi possível carregar os destaques.');
    } finally {
      setLoading(false);
    }
  }, [section]);

  useEffect(() => {
    loadItems();
  }, [loadItems]);

  return { items, loading, error, refresh: loadItems };
};