import { useState, useCallback, useEffect } from 'react';
import { HomeService } from '../services/HomeService';

export function useHomeCarousel(section: 'hero' | 'stats') {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchData = useCallback(async () => {
    try {
      const data = await HomeService.getHomeData();
      if (section === 'hero') {
        const heroItems = data.carousels?.filter(c => c.section === 'hero') || [];
        setItems(heroItems.sort((a, b) => a.order - b.order));
      } else if (section === 'stats') {
        const statsCarousels = data.carousels?.filter(c => c.section === 'stats') || [];
        setItems(statsCarousels.sort((a, b) => a.order - b.order));
      }
    } catch (err) {
      console.error(err);
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [section]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return { items, loading };
}