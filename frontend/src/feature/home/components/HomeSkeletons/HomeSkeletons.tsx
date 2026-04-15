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