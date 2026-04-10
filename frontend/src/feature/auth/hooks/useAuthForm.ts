import { useState, useMemo } from 'react';
import { useAuth } from './useAuth';
import { AlertService } from '@/core/alert/AlertService';
import type { RegisterRequestDTO } from '../types';

interface PasswordCriteria {
  minLen: boolean;
  hasUpper: boolean;
  hasNumber: boolean;
  hasSpecial: boolean;
}

export const useAuthForm = () => {
  const { register, loading } = useAuth();

  const [formData, setFormData] = useState({
    full_name: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const [touched, setTouched] = useState({
    email: false,
    confirmPassword: false,
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    const { name } = e.target;
    setTouched((prev) => ({
      ...prev,
      [name]: true,
    }));
  };

  const isEmailValid = useMemo(() => {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(formData.email);
  }, [formData.email]);

  const passwordCriteria = useMemo<PasswordCriteria>(() => {
    const pw = formData.password;
    return {
      minLen: pw.length >= 8,
      hasUpper: /[A-Z]/.test(pw),
      hasNumber: /[0-9]/.test(pw),
      hasSpecial: /[^A-Za-z0-9]/.test(pw),
    };
  }, [formData.password]);

  const isPasswordValid =
    passwordCriteria.minLen &&
    passwordCriteria.hasUpper &&
    passwordCriteria.hasNumber &&
    passwordCriteria.hasSpecial;

  const passwordsMatch = formData.password !== '' && formData.password === formData.confirmPassword;

  const isFormValid =
    formData.full_name.trim().length > 0 &&
    isEmailValid &&
    isPasswordValid &&
    passwordsMatch;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isFormValid || loading) return;

    const dto: RegisterRequestDTO = {
      full_name: formData.full_name,
      email: formData.email,
      password: formData.password,
    };

    const success = await register(dto);
    if (!success) {
       // A mensagem de erro específica já é tratada dentro do useAuth.ts com AlertService
       // Mas caso quiséssemos tratar alguma lógica extra de erro do form, faríamos aqui.
       // O requisito dizia "Utilize o AlertService.ts do core para exibir mensagens de erro caso o envio falhe."
       // No entanto, ao olharmos o código do useAuth.ts original:
       // ele já chama AlertService.error('Erro', ...); quando falha, e retorna false.
       // Apenas garantir que estamos retornando o status já é suficiente pois o useAuth alerta.
       console.error("Cadastro falhou");
    }
  };

  return {
    formData,
    handleChange,
    handleBlur,
    handleSubmit,
    isEmailValid,
    passwordCriteria,
    isPasswordValid,
    passwordsMatch,
    isFormValid,
    touched,
    loading
  };
};
