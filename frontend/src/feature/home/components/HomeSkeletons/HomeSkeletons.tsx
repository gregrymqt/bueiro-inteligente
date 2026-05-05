// HomeSkeletons.tsx
import styles from '@/pages/Home/Home.module.scss';

export function HeroSkeleton() {
  return <div className={styles.skeletonHero} aria-busy="true" aria-label="Carregando destaques" />;
}

export function HowItWorksSkeleton() {
  return (
    <div className={styles.skeletonGrid} aria-busy="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <div key={index} className={styles.stepItem}> {/* Reutilizando classe do componente real */}
          <div className={styles.skeletonCircle} style={{ margin: '0 auto 1.5rem' }} />
          <div className={`${styles.skeletonLine} ${styles.skeletonLineMedium}`} />
          <div className={styles.skeletonLine} style={{ marginTop: '1rem' }} />
          <div className={styles.skeletonLine} />
        </div>
      ))}
    </div>
  );
}

export function PricingSkeleton() {
  return (
    <div className={styles.skeletonGrid}>
      {[1, 2, 3].map((i) => (
        <div key={i} className={styles.skeletonCard} />
      ))}
    </div>
  );
}

export function ReviewsSkeleton() {
  return (
    <div className={styles.skeletonGrid} aria-busy="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <div key={index} className={styles.skeletonCard}>
          <div className={styles.skeletonLine} style={{ width: '40%', marginBottom: '1rem' }} />
          <div className={styles.skeletonLine} />
          <div className={styles.skeletonLine} style={{ width: '90%', marginTop: '0.5rem' }} />
          
          <div style={{ marginTop: '2rem', display: 'flex', gap: '1rem', alignItems: 'center' }}>
            <div className={styles.skeletonCircle} style={{ width: '2.5rem', height: '2.5rem' }} />
            <div style={{ flex: 1 }}>
              <div className={styles.skeletonLine} style={{ width: '60%' }} />
              <div className={`${styles.skeletonLine} ${styles.skeletonLineSmall}`} style={{ marginTop: '0.4rem' }} />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

export function HomeLoading() {
  return (
    <div className={styles.homeContainer}>
      <div className={styles.heroWrapper}>
        <HeroSkeleton />
      </div>
      
      <section className={styles.section}>
        <div className={styles.container}>
          <div className={`${styles.skeletonLine} ${styles.skeletonLineTitle}`} />
          <HowItWorksSkeleton />
        </div>
      </section>

      <section className={`${styles.section} ${styles.bgAlt}`}>
        <div className={styles.container}>
          <div className={`${styles.skeletonLine} ${styles.skeletonLineTitle}`} />
          <PricingSkeleton />
        </div>
      </section>
    </div>
  );
}