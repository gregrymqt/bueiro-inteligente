import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { Check } from "lucide-react";
import { Form } from "@/components/layout/Form";
import styles from "./RegisterForm.module.scss";
import { useAuth } from "../hooks/useAuth";
import type { RegisterRequestDTO } from "../types";

type RegisterFormValues = RegisterRequestDTO & {
  full_name: string;
  confirmPassword: string;
};

type PasswordRequirement = {
  key: string;
  label: string;
  test: (password: string) => boolean;
};

const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

const passwordRequirements: PasswordRequirement[] = [
  {
    key: "minLength",
    label: "8+ caracteres",
    test: (password) => password.length >= 8,
  },
  {
    key: "upperCase",
    label: "Letra maiúscula",
    test: (password) => /[A-Z]/.test(password),
  },
  {
    key: "number",
    label: "Um número",
    test: (password) => /[0-9]/.test(password),
  },
  {
    key: "special",
    label: "Caractere especial",
    test: (password) => /[!@#$%^&*]/.test(password),
  },
];

const getDefaultValues = (): RegisterFormValues => ({
  full_name: "",
  email: "",
  password: "",
  confirmPassword: "",
});

export const RegisterForm = () => {
  const { register: registerUser, loading } = useAuth();

  const methods = useForm<RegisterFormValues>({
    mode: "onChange",
    defaultValues: getDefaultValues(),
  });

  const [passwordValue, setPasswordValue] = useState(
    methods.getValues("password"),
  );

  useEffect(() => {
    // Mantém o feedback de senha sincronizado em tempo real via watch do react-hook-form.
    // eslint-disable-next-line react-hooks/incompatible-library
    const subscription = methods.watch((values, { name }) => {
      if (name === "password") {
        setPasswordValue(values.password ?? "");
      }
    });

    return () => subscription.unsubscribe();
  }, [methods]);

  const passwordChecks = useMemo(
    () =>
      passwordRequirements.map((requirement) => ({
        ...requirement,
        met: requirement.test(passwordValue),
      })),
    [passwordValue],
  );

  const isPasswordValid = passwordChecks.every(
    (requirement) => requirement.met,
  );

  const handleSubmit = async (values: RegisterFormValues) => {
    const payload: RegisterRequestDTO = {
      email: values.email.trim(),
      password: values.password,
      full_name: values.full_name.trim(),
    };

    await registerUser(payload);
  };

  return (
    <div className={styles.registerContainer}>
      <Form
        methods={methods}
        onSubmit={handleSubmit}
        className={styles.registerForm}
      >
        <h1 className={styles.registerFormTitle}>Crie sua Conta</h1>
        <p className={styles.registerFormSubtitle}>
          Inscreva-se para monitorar os bueiros em tempo real.
        </p>

        <Form.Input
          name="full_name"
          label="Nome Completo"
          type="text"
          placeholder="Seu nome"
          validation={{ required: "Nome completo é obrigatório." }}
        />

        <Form.Input
          name="email"
          label="E-mail"
          type="email"
          placeholder="seu@email.com"
          validation={{
            required: "E-mail é obrigatório.",
            pattern: {
              value: EMAIL_REGEX,
              message: "Por favor, insira um e-mail válido.",
            },
          }}
        />

        <Form.Input
          name="password"
          label="Senha"
          type="password"
          placeholder="Sua senha"
          validation={{
            required: "Senha é obrigatória.",
            validate: (value: string) =>
              passwordRequirements.every((requirement) =>
                requirement.test(value),
              ) || "A senha deve atender aos requisitos abaixo.",
          }}
        />

        <section
          className={styles.registerFormRequirements}
          aria-labelledby="password-requirements-title"
        >
          <p
            id="password-requirements-title"
            className={styles.registerFormRequirementsTitle}
          >
            Segurança da Senha
          </p>

          <ul
            className={styles.registerFormCriteriaContainer}
            aria-live="polite"
          >
            {passwordChecks.map((requirement) => {
              const isMet = requirement.met;

              return (
                <li
                  key={requirement.key}
                  className={`${styles.registerFormCriteriaItem} ${isMet ? styles.met : styles.unmet}`}
                >
                  {/* Substituímos o Circle por um Check condicional ou um marcador neutro */}
                  <div className={styles.indicatorWrapper}>
                    {isMet ? (
                      <Check
                        size={14}
                        className={styles.checkIcon}
                        aria-hidden="true"
                      />
                    ) : (
                      <span className={styles.bullet} aria-hidden="true" />
                    )}
                  </div>
                  <span className={styles.label}>{requirement.label}</span>
                </li>
              );
            })}
          </ul>
        </section>

        <Form.Input
          name="confirmPassword"
          label="Confirmar Senha"
          type="password"
          placeholder="Confirme sua senha"
          validation={{
            required: "Confirmação de senha é obrigatória.",
            deps: "password",
            validate: (value: string) =>
              value === passwordValue || "As senhas não coincidem.",
          }}
        />

        <Form.Actions className={styles.registerFormActions}>
          <Form.Submit
            isLoading={loading || methods.formState.isSubmitting}
            disabled={!methods.formState.isValid || !isPasswordValid}
            className={styles.registerFormButton}
          >
            Cadastrar
          </Form.Submit>
        </Form.Actions>

        <div className={styles.registerFormLoginLink}>
          <span>Já possui conta?</span> <Link to="/login">Faça login</Link>
        </div>
      </Form>
    </div>
  );
};
