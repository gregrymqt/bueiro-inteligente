import { FormProvider, useFormContext } from 'react-hook-form';
import type { FieldValues } from 'react-hook-form';
import styles from './Form.module.scss';
import type { FormProps, FormInputProps, FormSelectProps, FormTextareaProps, FormCheckboxProps, FormSubmitProps, FormActionsProps } from './Form.types';

export const Form = <T extends FieldValues>({
  methods,
  onSubmit,
  children,
  className = ''
}: FormProps<T>) => {
  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(onSubmit)} className={`${styles.form} ${className}`}>
        {children}
      </form>
    </FormProvider>
  );
};

export const FormInput = ({ name, label, type = 'text', validation, className = '', colSpan = 12, ...props }: FormInputProps) => {
  const { register, formState: { errors } } = useFormContext();
  const error = errors[name]?.message as string;
  const colClass = styles[`colSpan${colSpan}`];

  return (
    <div className={`${styles.field} ${colClass} ${className}`}>
      {label && (
        <label htmlFor={name}>
          {label} {validation?.required && <span className={styles.required}>*</span>}
        </label>
      )}
      <input
        id={name}
        type={type}
        className={error ? styles.errorInput : ''}
        {...register(name, validation)}
        {...props}
      />
      {error && <span className={styles.errorMsg}><i className="fas fa-exclamation-circle"></i> {error}</span>}
    </div>
  );
};

export const FormSelect = ({ name, label, options, validation, className = '', colSpan = 12, ...props }: FormSelectProps) => {
  const { register, formState: { errors } } = useFormContext();
  const error = errors[name]?.message as string;
  const colClass = styles[`colSpan${colSpan}`];

  return (
    <div className={`${styles.field} ${colClass} ${className}`}>
      {label && (
        <label htmlFor={name}>
          {label} {validation?.required && <span className={styles.required}>*</span>}
        </label>
      )}
      <select
        id={name}
        className={error ? styles.errorInput : ''}
        {...register(name, validation)}
        {...props}
      >
        <option value="">Selecione...</option>
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {error && <span className={styles.errorMsg}><i className="fas fa-exclamation-circle"></i> {error}</span>}
    </div>
  );
};

export const FormTextarea = ({ name, label, validation, className = '', colSpan = 12, ...props }: FormTextareaProps) => {
  const { register, formState: { errors } } = useFormContext();
  const error = errors[name]?.message as string;
  const colClass = styles[`colSpan${colSpan}`];

  return (
    <div className={`${styles.field} ${colClass} ${className}`}>
      {label && (
        <label htmlFor={name}>
          {label} {validation?.required && <span className={styles.required}>*</span>}
        </label>
      )}
      <textarea
        id={name}
        className={error ? styles.errorInput : ''}
        {...register(name, validation)}
        {...props}
      />
      {error && <span className={styles.errorMsg}><i className="fas fa-exclamation-circle"></i> {error}</span>}
    </div>
  );
};

export const FormCheckbox = ({ name, label, validation, className = '', colSpan = 12, ...props }: FormCheckboxProps) => {
  const { register, formState: { errors } } = useFormContext();
  const error = errors[name]?.message as string;
  const colClass = styles[`colSpan${colSpan}`];

  return (
    <div className={`${styles.field} ${styles.checkboxField} ${colClass} ${className}`}>
      <input
        id={name}
        type="checkbox"
        {...register(name, validation)}
        {...props}
      />
      <label htmlFor={name}>{label}</label>
      {error && <span className={styles.errorMsg}><i className="fas fa-exclamation-circle"></i> {error}</span>}
    </div>
  );
};

export const FormActions = ({ children, className = '' }: FormActionsProps) => {
  return <div className={`${styles.actions} ${className}`}>{children}</div>;
};

export const FormSubmit = ({ children, isLoading, className = '', ...props }: FormSubmitProps) => {
  return (
    <button type="submit" className={`${styles.submitBtn} ${className}`} disabled={isLoading} {...props}>
      {isLoading ? (
        <span><i className="fas fa-spinner fa-spin"></i> Processando...</span>
      ) : (
        children
      )}
    </button>
  );
};

Form.Input = FormInput;
Form.Select = FormSelect;
Form.Textarea = FormTextarea;
Form.Checkbox = FormCheckbox;
Form.Actions = FormActions;
Form.Submit = FormSubmit;
