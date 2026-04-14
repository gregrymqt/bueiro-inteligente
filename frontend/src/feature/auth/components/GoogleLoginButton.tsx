import React from 'react';
import { Button } from '@/components/ui/Button/Button';
import { resolveBackendBaseUrl } from '@/core/http/environment';

interface GoogleLoginButtonProps {
  className?: string;
}

export const GoogleLoginButton: React.FC<GoogleLoginButtonProps> = ({ className = '' }) => {
  const handleGoogleLogin = () => {
    const backendBaseUrl = resolveBackendBaseUrl();
    const googleLoginUrl = new URL('/auth/google-login', backendBaseUrl);
    googleLoginUrl.searchParams.set('frontend_redirect', window.location.origin);
    window.location.href = googleLoginUrl.toString();
  };

  return (
    <Button
      type="button"
      variant="secondary"
      size="lg"
      leftIcon={<i className="fab fa-google"></i>}
      onClick={handleGoogleLogin}
      className={className}
    >
      Entrar com Google
    </Button>
  );
};