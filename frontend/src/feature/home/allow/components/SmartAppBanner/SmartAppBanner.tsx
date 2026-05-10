import { useState } from 'react';
import styles from './SmartAppBanner.module.scss';

const BANNER_DISMISSED_KEY = 'smart-app-banner:dismissed';

type NavigatorWithUserAgentData = Navigator & {
  userAgentData?: {
    platform?: string;
  };
};

function resolveDownloadUrl(): string | null {
  const rawUrl = import.meta.env.VITE_ANDROID_APP_DOWNLOAD_URL?.trim();

  if (!rawUrl) {
    return null;
  }

  const normalizedUrl = rawUrl.replace(/\/+$/, '');
  return normalizedUrl.length > 0 ? normalizedUrl : null;
}

function isAndroidDevice(): boolean {
  if (typeof window === 'undefined') {
    return false;
  }

  const navigatorWithUserAgentData = window.navigator as NavigatorWithUserAgentData;
  const { userAgent, userAgentData } = navigatorWithUserAgentData;

  return Boolean(userAgentData?.platform === 'Android' || /Android/i.test(userAgent));
}

function hasDismissedBanner(): boolean {
  if (typeof window === 'undefined') {
    return false;
  }

  try {
    return window.localStorage.getItem(BANNER_DISMISSED_KEY) === 'true';
  } catch {
    return false;
  }
}

function setDismissedBanner(): void {
  if (typeof window === 'undefined') {
    return;
  }

  try {
    window.localStorage.setItem(BANNER_DISMISSED_KEY, 'true');
  } catch {
    // Ignora falhas de armazenamento para não bloquear o CTA.
  }
}

export function SmartAppBanner() {
  const downloadUrl = resolveDownloadUrl();
  const [isVisible, setIsVisible] = useState(() => {
    if (!downloadUrl) {
      return false;
    }

    return isAndroidDevice() && !hasDismissedBanner();
  });

  if (!isVisible || !downloadUrl) {
    return null;
  }

  const handleDismiss = () => {
    setDismissedBanner();
    setIsVisible(false);
  };

  return (
    <aside className={styles.banner} aria-label="Banner de download do aplicativo Android">
      <div className={styles.content}>
        <span className={styles.badge}>Android</span>

        <div className={styles.textGroup}>
          <p className={styles.kicker}>Versão para celular</p>
          <h2 className={styles.title}>Baixe o app do Bueiro Inteligente</h2>
          <p className={styles.description}>
            Instale o APK hospedado no Supabase e receba os alertas do monitoramento direto no seu Android.
          </p>
        </div>
      </div>

      <div className={styles.actions}>
        <a
          className={styles.primaryAction}
          href={downloadUrl}
          target="_blank"
          rel="noopener noreferrer"
          download
        >
          Baixar APK
        </a>

        <button
          type="button"
          className={styles.dismissButton}
          onClick={handleDismiss}
          aria-label="Fechar banner de download do Android"
        >
          Agora não
        </button>
      </div>
    </aside>
  );
}