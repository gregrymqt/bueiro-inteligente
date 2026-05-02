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