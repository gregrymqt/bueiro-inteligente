import React from 'react';
import { Link } from 'react-router-dom';
import { Check, Circle } from 'lucide-react';
import styles from './RegisterForm.module.scss';
import { useAuthForm } from '../hooks/useAuthForm';

export const RegisterForm: React.FC = () => {
  const {
    formData,
    handleChange,
    handleBlur,
    handleSubmit,
    isEmailValid,
    passwordCriteria,
    isFormValid,
    touched,
    loading
  } = useAuthForm();

  return (
    <div className={styles.registerContainer}>
      <form className={styles.registerForm} onSubmit={handleSubmit}>
        <h1 className={styles.registerFormTitle}>Crie sua Conta</h1>
        <p className={styles.registerFormSubtitle}>Inscreva-se para monitorar os bueiros em tempo real.</p>

        <div className={styles.registerFormField}>
          <label htmlFor="full_name">Nome Completo</label>
          <input
            id="full_name"
            name="full_name"
            type="text"
            placeholder="Seu nome"
            value={formData.full_name}
            onChange={handleChange}
            disabled={loading}
          />
        </div>

        <div className={styles.registerFormField}>
          <label htmlFor="email">E-mail</label>
          <input
            id="email"
            name="email"
            type="email"
            placeholder="seu@email.com"
            value={formData.email}
            onChange={handleChange}
            onBlur={handleBlur}
            disabled={loading}
            className={touched.email && !isEmailValid ? styles.error : ''}
          />
        </div>

        <div className={styles.registerFormField}>
          <label htmlFor="password">Senha</label>
          <input
            id="password"
            name="password"
            type="password"
            placeholder="Sua senha"
            value={formData.password}
            onChange={handleChange}
            disabled={loading}
          />
        </div>

        {/* Real-time Password Feedback */}
        <div className={styles.registerFormCriteriaContainer}>
          <div className={`${styles.registerFormCriteriaItem} ${passwordCriteria.minLen ? styles.met : ''}`}>
            {passwordCriteria.minLen ? <Check /> : <Circle />} Mínimo 8 caracteres
          </div>
          <div className={`${styles.registerFormCriteriaItem} ${passwordCriteria.hasUpper ? styles.met : ''}`}>
            {passwordCriteria.hasUpper ? <Check /> : <Circle />} Letra maiúscula
          </div>
          <div className={`${styles.registerFormCriteriaItem} ${passwordCriteria.hasNumber ? styles.met : ''}`}>
            {passwordCriteria.hasNumber ? <Check /> : <Circle />} Um número
          </div>
          <div className={`${styles.registerFormCriteriaItem} ${passwordCriteria.hasSpecial ? styles.met : ''}`}>
            {passwordCriteria.hasSpecial ? <Check /> : <Circle />} Caractere especial
          </div>
        </div>

        <div className={styles.registerFormField}>
          <label htmlFor="confirmPassword">Confirmar Senha</label>
          <input
            id="confirmPassword"
            name="confirmPassword"
            type="password"
            placeholder="Confirme sua senha"
            value={formData.confirmPassword}
            onChange={handleChange}
            onBlur={handleBlur}
            disabled={loading}
            className={touched.confirmPassword && formData.password !== formData.confirmPassword ? styles.error : ''}
          />
        </div>

        <button
          type="submit"
          className={styles.registerFormButton}
          disabled={!isFormValid || loading}
        >
          {loading ? 'Processando...' : 'Cadastrar'}
        </button>

        <div className={styles.registerFormLoginLink}>
          <span>Já possui conta?</span> <Link to="/login">Faça login</Link>
        </div>
      </form>
    </div>
  );
};
