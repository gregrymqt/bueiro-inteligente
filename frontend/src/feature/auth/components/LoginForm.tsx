import React from 'react';
import { Link } from 'react-router-dom';
import './LoginForm.scss';
import { useAuth } from '../hooks/useAuth';
import { GoogleLoginButton } from './GoogleLoginButton';
import { GenericForm, type FormField } from '@/components/layout/Form/GenericForm';
import { type LoginRequestDTO } from '../types';

export const LoginForm: React.FC = () => {
  const { login, loading, error } = useAuth();

  const fields: FormField<LoginRequestDTO>[] = [
    {
      name: 'email',
      label: 'E-mail',
      type: 'email',
      placeholder: 'seu@email.com',
      validation: { required: 'E-mail é obrigatório' }
    },
    {
      name: 'password',
      label: 'Senha',
      type: 'password',
      placeholder: '••••••••',
      validation: { 
        required: 'Senha é obrigatória'
      }
    }
  ];

  const onSubmit = async (data: LoginRequestDTO) => {
    await login(data);
  };

  return (
    <div className="login-container">
      <div className="login-form">
        <h1 className="login-form__title">Acesso ao Sistema</h1>
        <p className="login-form__subtitle">Monitore o ecossistema de bueiros em tempo real.</p>

        {error && <div className="login-form__error">{error}</div>}

        <GenericForm<LoginRequestDTO>
          fields={fields}
          onSubmit={onSubmit}
          isLoading={loading}
          submitText="Entrar"
        >
          <div className="login-form__register-link">
            <span>Não possui conta?</span> <Link to="/register">Crie uma aqui</Link>
          </div>

          <div className="login-form__social">
            <div className="login-form__divider" aria-hidden="true">
              <span>ou</span>
            </div>

            <GoogleLoginButton className="login-form__google-button" />
          </div>
        </GenericForm>
      </div>
    </div>
  );
};

