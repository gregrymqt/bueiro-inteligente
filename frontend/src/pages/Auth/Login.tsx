import React from 'react';
import { LoginForm } from '@/feature/auth/components/LoginForm';
import './Login.scss';

export const Login: React.FC = () => {
  return (
    <div className="login-page-wrapper">
      <div className="login-page-container">
        <LoginForm />
      </div>
    </div>
  );
};