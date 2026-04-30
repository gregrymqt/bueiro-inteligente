import styles from '@/pages/Home/Home.module.scss';

export function HeroSkeleton() {
  return <div className={styles.heroSkeleton} aria-busy="true" aria-label="Carregando destaques" />;
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
    <div className={styles.statsSkeletonGrid} aria-busy="true" aria-label="Carregando planos">
      {Array.from({ length: 3 }).map((_, index) => (
        <div key={`pricing-skeleton-${index}`} className={styles.statsSkeletonCard}>
          <div className={styles.statsSkeletonHeader}>
            <div className={styles.statsSkeletonMeta}>
              <div className={styles.statsSkeletonLine} style={{ height: '1.5rem', width: '50%' }} />
              <div className={styles.statsSkeletonLine} style={{ height: '2rem', width: '70%', marginTop: '0.5rem' }} />
            </div>
          </div>
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineWide}`} style={{ marginTop: '1rem' }} />
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineShort}`} />
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineWide}`} />
        </div>
      ))}
    </div>
  );
}

export function ReviewsSkeleton() {
  return (
    <div className={styles.statsSkeletonGrid} aria-busy="true" aria-label="Carregando avaliações">
      {Array.from({ length: 3 }).map((_, index) => (
        <div key={`review-skeleton-${index}`} className={styles.statsSkeletonCard}>
          <div className={styles.statsSkeletonHeader}>
            <div className={styles.statsSkeletonIcon} style={{ borderRadius: '50%' }} />
            <div className={styles.statsSkeletonMeta}>
              <div className={styles.statsSkeletonLine} />
              <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineShort}`} />
            </div>
          </div>
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineWide}`} style={{ marginTop: '1rem' }} />
          <div className={`${styles.statsSkeletonLine} ${styles.statsSkeletonLineWide}`} />
        </div>
      ))}
    </div>
  );
}
