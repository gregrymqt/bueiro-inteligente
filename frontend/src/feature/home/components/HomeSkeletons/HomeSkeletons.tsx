import styles from '@/pages/Home/Home.module.scss';

export function HeroSkeleton() {
  return <div className={styles.heroSkeleton} aria-busy="true" aria-label="Carregando destaques" />;
}

export function HowItWorksSkeleton() {
  return (
    <div className={styles.statsSkeletonGrid} aria-busy="true" aria-label="Carregando como funciona">
      {Array.from({ length: 3 }).map((_, index) => (
        <div key={`how-it-works-skeleton-${index}`} className={styles.statsSkeletonCard}>
          <div className={styles.statsSkeletonHeader}>
            <div className={styles.statsSkeletonIcon} style={{ borderRadius: '50%', width: '3rem', height: '3rem' }} />
            <div className={styles.statsSkeletonMeta}>
              <div className={styles.statsSkeletonLine} style={{ height: '1.2rem', width: '80%' }} />
            </div>
          </div>
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineWide}`} style={{ marginTop: '1rem' }} />
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineWide}`} />
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineShort}`} />
        </div>
      ))}
    </div>
  );
}

export function StatsSkeleton() {
  return (
    <div className={styles.statsSkeletonGrid} aria-busy="true" aria-label="Carregando métricas">
      {Array.from({ length: 3 }).map((_, index) => (
        <div key={`stats-skeleton-${index}`} className={styles.statsSkeletonCard}>
          <div className={styles.statsSkeletonHeader}>
            <div className={styles.statsSkeletonIcon} />
            <div className={styles.statsSkeletonMeta}>
              <div className={styles.statsSkeletonLine} />
              <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineShort}`} />
            </div>
          </div>

          <div className={styles.statsSkeletonValue} />
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineWide}`} />
        </div>
      ))}
    </div>
  );
}

export function PricingSkeleton() {
  return (
    <div className={styles.statsSkeletonGrid}>
      {[1, 2, 3].map((i) => (
        <div key={i} className={styles.statsSkeletonCard} style={{ height: '350px' }} />
      ))}
    </div>
  );
}

export function ReviewsSkeleton() {
  return (
    <div className={styles.statsSkeletonGrid} aria-busy="true" aria-label="Carregando avaliações">
      {Array.from({ length: 3 }).map((_, index) => (
        <div key={`review-skeleton-${index}`} className={styles.statsSkeletonCard}>
          {/* Estrelas */}
          <div className={styles.statsSkeletonHeader} style={{ marginBottom: '0.5rem' }}>
            <div className={styles.statsSkeletonLine} style={{ width: '40%', height: '1rem' }} />
          </div>
          
          {/* Corpo do comentário */}
          <div className={styles.statsSkeletonLine} style={{ width: '100%' }} />
          <div className={styles.statsSkeletonLine} style={{ width: '90%' }} />
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineShort}`} />

          {/* Autor (Avatar + Texto) */}
          <div className={styles.statsSkeletonHeader} style={{ marginTop: '1.5rem', borderTop: '1px solid #eee', paddingTop: '1rem' }}>
            <div className={styles.statsSkeletonIcon} style={{ width: '2.5rem', height: '2.5rem' }} />
            <div className={styles.statsSkeletonMeta}>
              <div className={styles.statsSkeletonLine} style={{ width: '60%' }} />
              <div className={styles.statsSkeletonLineShort} style={{ height: '0.6rem' }} />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

// O componente que você vai chamar na sua página Home.tsx[cite: 5]
export function HomeLoading() {
  return (
    <div className={styles.homeContainer}>
      <HeroSkeleton />
      
      <section className={styles.section}>
        <div className={styles.container}>
          <div className={styles.statsSkeletonLine} style={{ width: '200px', height: '2rem', margin: '0 auto 2rem' }} />
          <HowItWorksSkeleton />
        </div>
      </section>

      <section className={styles.section} style={{ backgroundColor: '#f9fafb' }}>
        <div className={styles.container}>
          <div className={styles.statsSkeletonLine} style={{ width: '200px', height: '2rem', margin: '0 auto 2rem' }} />
          <PricingSkeleton />
        </div>
      </section>

      <section className={styles.section}>
        <div className={styles.container}>
          <div className={styles.statsSkeletonLine} style={{ width: '200px', height: '2rem', margin: '0 auto 2rem' }} />
          <ReviewsSkeleton />
        </div>
      </section>
    </div>
  );
}