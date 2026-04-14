import React from 'react';
import { Button } from '@/components/ui/Button/Button';
import { resolveBackendBaseUrl, isMockDataSourceEnabled } from '@/core/http/environment';
import { useAuth } from '../hooks/useAuth';
import { mockDemoCredentials } from '../mocks/authMocks';

interface GoogleLoginButtonProps {
  className?: string;
}

export const GoogleLoginButton: React.FC<GoogleLoginButtonProps> = ({ className = '' }) => {
  const { login, loading } = useAuth();
  const isDemoMode = isMockDataSourceEnabled();

  const handleGoogleLogin = async () => {
    if (!isDemoMode) {
      const backendBaseUrl = resolveBackendBaseUrl();
      const googleLoginUrl = new URL('/auth/google-login', backendBaseUrl);
      googleLoginUrl.searchParams.set('frontend_redirect', window.location.origin);
      window.location.href = googleLoginUrl.toString();
      return;
    }

    await login(mockDemoCredentials);
  };

  return (
    <Button
      type="button"
      variant="secondary"
      size="lg"
      leftIcon={<i className="fab fa-google"></i>}
      onClick={handleGoogleLogin}
      className={className}
      isLoading={isDemoMode && loading}
    >
      Entrar com Google
    </Button>
  );
};