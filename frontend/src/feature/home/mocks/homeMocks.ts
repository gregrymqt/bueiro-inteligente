import { withMockLatency } from '@/core/mock/mockDelay';
import type {
  CarouselContent,
  CarouselCreatePayload,
  CarouselUpdatePayload,
  HomeDataResponse,
  StatCardContent,
  StatCardCreatePayload,
  StatCardUpdatePayload,
  PricingPlan,
  UserReview,
} from '../types';

const createHeroArtwork = (title: string, subtitle: string, startColor: string, endColor: string, glowColor: string): string => {
  const svg = `
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1600 900" role="img" aria-label="${title}">
      <defs>
        <linearGradient id="background" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="${startColor}" />
          <stop offset="100%" stop-color="${endColor}" />
        </linearGradient>
        <radialGradient id="glow" cx="28%" cy="28%" r="72%">
          <stop offset="0%" stop-color="${glowColor}" stop-opacity="0.58" />
          <stop offset="100%" stop-color="${glowColor}" stop-opacity="0" />
        </radialGradient>
      </defs>
      <rect width="1600" height="900" fill="url(#background)" />
      <circle cx="260" cy="180" r="320" fill="url(#glow)" />
      <circle cx="1280" cy="720" r="340" fill="url(#glow)" />
      <path d="M0 660C180 610 300 750 470 706C650 660 758 520 920 544C1082 568 1206 724 1600 642L1600 900L0 900Z" fill="rgba(255,255,255,0.11)" />
      <path d="M120 676H1480" stroke="rgba(255,255,255,0.24)" stroke-width="6" stroke-linecap="round" />
      <path d="M200 548C320 512 420 624 550 590C694 552 770 408 910 430C1040 452 1118 582 1262 534C1340 508 1408 470 1498 480" fill="none" stroke="rgba(255,255,255,0.38)" stroke-width="10" stroke-linecap="round" />
      <text x="96" y="150" fill="#FFFFFF" font-family="Inter, Segoe UI, Arial, sans-serif" font-size="52" font-weight="700">Bueiro Inteligente</text>
      <text x="96" y="252" fill="#FFFFFF" font-family="Inter, Segoe UI, Arial, sans-serif" font-size="86" font-weight="800">${title}</text>
      <text x="96" y="326" fill="rgba(255,255,255,0.92)" font-family="Inter, Segoe UI, Arial, sans-serif" font-size="34">${subtitle}</text>
      <rect x="96" y="384" width="264" height="72" rx="36" fill="rgba(255,255,255,0.16)" />
      <text x="146" y="432" fill="#FFFFFF" font-family="Inter, Segoe UI, Arial, sans-serif" font-size="30" font-weight="700">Demonstração UI/UX</text>
    </svg>
  `;

  return `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}`;
};

const cloneCarousel = (item: CarouselContent): CarouselContent => ({
  ...item,
});

const cloneStat = (item: StatCardContent): StatCardContent => ({
  ...item,
});

const sortByOrder = <T extends { order: number }>(items: T[]): T[] =>
  [...items].sort((left, right) => left.order - right.order);

const initialCarousels: CarouselContent[] = [
  {
    id: 'carousel-hero-01',
    title: 'Solução Integrada: ESP32 + Bueiro',
    subtitle: 'Sensores IoT de alta precisão (ESP32) instalados diretamente nos bueiros para monitoramento urbano contínuo.',
    image_url: createHeroArtwork('Solução Integrada: ESP32 + Bueiro', 'Sensores IoT de alta precisão (ESP32) instalados diretamente nos bueiros para monitoramento urbano contínuo.', '#0f172a', '#1d4ed8', '#22d3ee'),
    action_url: '/dashboard',
    order: 1,
    section: 'hero',
  },
  {
    id: 'carousel-hero-02',
    title: 'Alertas antes da rua alagar',
    subtitle: 'Visual limpo para destacar risco, prioridade e resposta rápida.',
    image_url: createHeroArtwork('Alertas antes da rua alagar', 'Visual limpo para destacar risco, prioridade e resposta rápida.', '#111827', '#0f766e', '#34d399'),
    action_url: '/dashboard',
    order: 2,
    section: 'hero',
  },
  {
    id: 'carousel-hero-03',
    title: 'Gestão integrada da equipe',
    subtitle: 'Administração, monitoramento e relatórios organizados no mesmo fluxo.',
    image_url: createHeroArtwork('Gestão integrada da equipe', 'Administração, monitoramento e relatórios organizados no mesmo fluxo.', '#1f2937', '#7c2d12', '#fb923c'),
    action_url: '/admin/home',
    order: 3,
    section: 'hero',
  },
  {
    id: 'carousel-alert-01',
    title: 'Chuva forte prevista para 17h',
    subtitle: 'A vigilância do ponto crítico foi reforçada pela operação.',
    image_url: createHeroArtwork('Chuva forte prevista para 17h', 'A vigilância do ponto crítico foi reforçada pela operação.', '#111827', '#a16207', '#fde68a'),
    action_url: '/dashboard',
    order: 1,
    section: 'alerts',
  },
  {
    id: 'carousel-alert-02',
    title: 'Bueiro 03 em manutenção preventiva',
    subtitle: 'Equipe acionada e rota alternativa disponível no painel administrativo.',
    image_url: createHeroArtwork('Bueiro 03 em manutenção preventiva', 'Equipe acionada e rota alternativa disponível no painel administrativo.', '#0f172a', '#b91c1c', '#f87171'),
    action_url: '/admin/drains',
    order: 2,
    section: 'alerts',
  },
  {
    id: 'carousel-stat-01',
    title: 'Bueiros ativos',
    subtitle: '28',
    image_url: createHeroArtwork('28', 'Bueiros ativos nesta simulação', '#0f172a', '#14532d', '#4ade80'),
    action_url: '/dashboard',
    order: 1,
    section: 'stats',
  },
  {
    id: 'carousel-stat-02',
    title: 'Alertas críticos',
    subtitle: '3',
    image_url: createHeroArtwork('3', 'Alertas críticos nesta janela', '#1f2937', '#b45309', '#fbbf24'),
    action_url: '/dashboard',
    order: 2,
    section: 'stats',
  },
  {
    id: 'carousel-stat-03',
    title: 'Última sincronização',
    subtitle: '2 min',
    image_url: createHeroArtwork('2 min', 'Última sincronização do painel', '#111827', '#1d4ed8', '#60a5fa'),
    action_url: '/dashboard',
    order: 3,
    section: 'stats',
  },
];


const initialPlans: PricingPlan[] = [
  {
    id: 'plan-01',
    name: 'Básico',
    price: 'R$ 499/mês',
    description: 'Monitoramento de até 10 bueiros.',
    features: ['Dashboard em tempo real', 'Alertas por e-mail', 'Suporte comercial'],
    isPopular: false,
    order: 1,
  },
  {
    id: 'plan-02',
    name: 'Avançado',
    price: 'R$ 1299/mês',
    description: 'Monitoramento de até 50 bueiros.',
    features: ['Dashboard completo', 'Alertas SMS e App', 'Suporte 24/7 prioritário', 'Integração via API'],
    isPopular: true,
    order: 2,
  },
  {
    id: 'plan-03',
    name: 'Enterprise',
    price: 'Sob Consulta',
    description: 'Solução dedicada para cidades inteiras.',
    features: ['Bueiros ilimitados', 'Instalação in loco', 'Gerente de conta', 'SLA garantido'],
    isPopular: false,
    order: 3,
  },
];

const initialReviews: UserReview[] = [
  {
    id: 'review-01',
    authorName: 'Prefeitura de São Paulo',
    authorRole: 'Secretaria de Infraestrutura',
    content: 'O sistema revolucionou a forma como lidamos com alagamentos nas marginais.',
    rating: 5,
    order: 1,
  },
  {
    id: 'review-02',
    authorName: 'Carlos Mendes',
    authorRole: 'Engenheiro Civil',
    content: 'Instalação dos ESP32 foi simples e a telemetria é muito confiável.',
    rating: 4,
    order: 2,
  },
  {
    id: 'review-03',
    authorName: 'Defesa Civil',
    authorRole: 'Operações',
    content: 'Conseguimos antecipar as equipes de limpeza antes das fortes chuvas.',
    rating: 5,
    order: 3,
  },
];

const initialStats: StatCardContent[] = [
  {
    id: 'stat-01',
    title: 'Cobertura da rede',
    value: '94%',
    description: 'Bueiros com telemetria confirmada no painel mockado.',
    icon_name: 'Gauge',
    color: 'success',
    order: 1,
  },
  {
    id: 'stat-02',
    title: 'Alertas nas últimas 24h',
    value: '12',
    description: 'Ocorrências registradas e tratadas pela operação.',
    icon_name: 'TriangleAlert',
    color: 'warning',
    order: 2,
  },
  {
    id: 'stat-03',
    title: 'Ocorrências críticas',
    value: '1',
    description: 'Equipe acionada em campo com prioridade máxima.',
    icon_name: 'ShieldAlert',
    color: 'danger',
    order: 3,
  },
  {
    id: 'stat-04',
    title: 'Tempo médio de resposta',
    value: '8 min',
    description: 'Entre a emissão do alerta e a triagem inicial.',
    icon_name: 'Clock3',
    color: 'success',
    order: 4,
  },
];

let carouselSequence = initialCarousels.length + 1;
let statSequence = initialStats.length + 1;

let homeState: HomeDataResponse = {
  carousels: initialCarousels.map(cloneCarousel),
  stats: initialStats.map(cloneStat),
  plans: initialPlans,
  reviews: initialReviews,
};

const createCarouselId = (): string => `carousel-${String(carouselSequence++).padStart(2, '0')}`;
const createStatId = (): string => `stat-${String(statSequence++).padStart(2, '0')}`;

const snapshotHomeData = (): HomeDataResponse => ({
  carousels: sortByOrder(homeState.carousels).map(cloneCarousel),
  stats: sortByOrder(homeState.stats).map(cloneStat),
  plans: sortByOrder(homeState.plans),
  reviews: sortByOrder(homeState.reviews),
});

const replaceHomeCarousels = (carousels: CarouselContent[]): void => {
  homeState = {
    ...homeState,
    carousels: carousels.map(cloneCarousel),
  };
};

const replaceHomeStats = (stats: StatCardContent[]): void => {
  homeState = {
    ...homeState,
    stats: stats.map(cloneStat),
  };
};

export const resetMockHomeData = (): void => {
  carouselSequence = initialCarousels.length + 1;
  statSequence = initialStats.length + 1;
  homeState = {
    carousels: initialCarousels.map(cloneCarousel),
    stats: initialStats.map(cloneStat),
    plans: initialPlans,
    reviews: initialReviews,
  };
};

export const getMockHomeData = async (): Promise<HomeDataResponse> =>
  withMockLatency(() => snapshotHomeData(), 260);

export const createMockCarouselItem = async (payload: CarouselCreatePayload): Promise<CarouselContent> =>
  withMockLatency(() => {
    const createdItem: CarouselContent = {
      id: createCarouselId(),
      image_url: 'https://via.placeholder.com/800x400?text=Mock+Image', // Fallback for image_url
      ...payload,
    };

    replaceHomeCarousels([...homeState.carousels, createdItem]);

    return cloneCarousel(createdItem);
  }, 280);

export const updateMockCarouselItem = async (id: string, payload: CarouselUpdatePayload): Promise<CarouselContent> =>
  withMockLatency(() => {
    const currentItem = homeState.carousels.find((item) => item.id === id);

    if (!currentItem) {
      throw new Error(`Banner ${id} não encontrado.`);
    }

    const updatedItem: CarouselContent = {
      ...currentItem,
      ...payload,
    };

    replaceHomeCarousels(homeState.carousels.map((item) => (item.id === id ? updatedItem : item)));

    return cloneCarousel(updatedItem);
  }, 280);

export const deleteMockCarouselItem = async (id: string): Promise<void> =>
  withMockLatency(() => {
    const existingItem = homeState.carousels.find((item) => item.id === id);

    if (!existingItem) {
      throw new Error(`Banner ${id} não encontrado.`);
    }

    replaceHomeCarousels(homeState.carousels.filter((item) => item.id !== id));
  }, 220);

export const createMockStatCard = async (payload: StatCardCreatePayload): Promise<StatCardContent> =>
  withMockLatency(() => {
    const createdStat: StatCardContent = {
      id: createStatId(),
      ...payload,
    };

    replaceHomeStats([...homeState.stats, createdStat]);

    return cloneStat(createdStat);
  }, 280);

export const updateMockStatCard = async (id: string, payload: StatCardUpdatePayload): Promise<StatCardContent> =>
  withMockLatency(() => {
    const currentStat = homeState.stats.find((item) => item.id === id);

    if (!currentStat) {
      throw new Error(`Card ${id} não encontrado.`);
    }

    const updatedStat: StatCardContent = {
      ...currentStat,
      ...payload,
    };

    replaceHomeStats(homeState.stats.map((item) => (item.id === id ? updatedStat : item)));

    return cloneStat(updatedStat);
  }, 280);

export const deleteMockStatCard = async (id: string): Promise<void> =>
  withMockLatency(() => {
    const existingStat = homeState.stats.find((item) => item.id === id);

    if (!existingStat) {
      throw new Error(`Card ${id} não encontrado.`);
    }

    replaceHomeStats(homeState.stats.filter((item) => item.id !== id));
  }, 220);