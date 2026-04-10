/**
 * Form Components - Composition Pattern with React Hook Form
 * 
 * Usage Example:
 * 
 * <Form onSubmit={handleSubmit} defaultValues={defaultValues}>
 *   <FormInput name="email" label="Email" type="email" validation={{ required: "Email obrigatório" }} />
 *   <FormInput name="password" label="Senha" type="password" />
 *   <FormSelect name="role" label="Função" options={roleOptions} />
 *   <FormTextarea name="bio" label="Biografia" />
 *   <FormCheckbox name="terms" label="Aceito os termos" />
 *   <FormActions>
 *     <button type="submit">Enviar</button>
 *   </FormActions>
 * </Form>
 */

import { type ReactNode } from 'react';
import  { 
  useForm, 
  FormProvider,
  useFormContext,
  type SubmitHandler, 
  type FieldValues, 
  type Path, 
  type RegisterOptions, 
  type DefaultValues, 
  type FieldError,
  type UseFormReturn 
} from 'react-hook-form';
import styles from './GenericForm.module.scss';

// ============ FORM WRAPPER (Provider) ============

interface FormProps<T extends FieldValues> {
  children: ReactNode;
  onSubmit: SubmitHandler<T>;
  defaultValues?: DefaultValues<T>;
  className?: string;
  formMethods?: UseFormReturn<T>; // Aceita formMethods externo (opcional)
}

export function Form<T extends FieldValues>({ 
  children, 
  onSubmit, 
  defaultValues,
  className = '',
  formMethods
}: FormProps<T>) {
  // Cria hooks internos de forma incondicional
  const internalMethods = useForm<T>({ defaultValues });

  // Usa formMethods externo se fornecido, senão usa o interno
  const methods = formMethods || internalMethods;

  return (
    <FormProvider {...methods}>
      <form 
        className={`${styles.form} ${className}`} 
        onSubmit={methods.handleSubmit(onSubmit)}
      >
        <div className={styles.grid}>
          {children}
        </div>
      </form>
    </FormProvider>
  );
}

// ============ FORM INPUT ============

interface FormInputProps<T extends FieldValues> {
  name: Path<T>;
  label: string;
  type?: 'text' | 'email' | 'password' | 'number' | 'date' | 'file';
  placeholder?: string;
  validation?: RegisterOptions<T, Path<T>>;
  rules?: RegisterOptions<T, Path<T>>; // Alias para validation (compatibilidade)
  colSpan?: 3 | 4 | 6 | 12;
  disabled?: boolean;
  accept?: string; // Para file input
  multiple?: boolean; // Para file input
}

export function FormInput<T extends FieldValues>({
  name,
  label,
  type = 'text',
  placeholder,
  validation,
  rules,
  colSpan = 12,
  disabled,
  accept,
  multiple
}: FormInputProps<T>) {
  const { register, formState: { errors } } = useFormContext<T>();
  const fieldError = errors[name] as FieldError | undefined;
  const errorMessage = fieldError?.message;
  
  // Usa 'rules' se fornecido, senão usa 'validation' (retrocompatibilidade)
  const registerOptions = rules || validation;

  return (
    <div className={`${styles.field} ${styles[`colSpan${colSpan}`]}`}>
      <label htmlFor={String(name)}>
        {label}
        {registerOptions?.required && <span className={styles.required}>*</span>}
      </label>
      <input
        id={String(name)}
        type={type}
        placeholder={placeholder}
        disabled={disabled}
        accept={accept}
        multiple={multiple}
        className={errorMessage ? styles.error : ''}
        {...register(name, registerOptions)}
      />
      {errorMessage && (
        <span className={styles.errorMsg}>
          <i className="fas fa-exclamation-circle"></i> {errorMessage}
        </span>
      )}
    </div>
  );
}

// ============ FORM SELECT ============

interface FormSelectProps<T extends FieldValues> {
  name: Path<T>;
  label: string;
  options: { label: string; value: string | number }[];
  validation?: RegisterOptions<T, Path<T>>;
  colSpan?: 3 | 4 | 6 | 12;
  disabled?: boolean;
  placeholder?: string;
}

export function FormSelect<T extends FieldValues>({
  name,
  label,
  options,
  validation,
  colSpan = 12,
  disabled,
  placeholder = "Selecione..."
}: FormSelectProps<T>) {
  const { register, formState: { errors } } = useFormContext<T>();
  const fieldError = errors[name] as FieldError | undefined;
  const errorMessage = fieldError?.message;

  return (
    <div className={`${styles.field} ${styles[`colSpan${colSpan}`]}`}>
      <label htmlFor={String(name)}>
        {label}
        {validation?.required && <span className={styles.required}>*</span>}
      </label>
      <select
        id={String(name)}
        disabled={disabled}
        className={errorMessage ? styles.error : ''}
        {...register(name, validation)}
      >
        <option value="">{placeholder}</option>
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {errorMessage && (
        <span className={styles.errorMsg}>
          <i className="fas fa-exclamation-circle"></i> {errorMessage}
        </span>
      )}
    </div>
  );
}

// ============ FORM TEXTAREA ============

interface FormTextareaProps<T extends FieldValues> {
  name: Path<T>;
  label: string;
  placeholder?: string;
  validation?: RegisterOptions<T, Path<T>>;
  colSpan?: 3 | 4 | 6 | 12;
  disabled?: boolean;
  rows?: number;
}

export function FormTextarea<T extends FieldValues>({
  name,
  label,
  placeholder,
  validation,
  colSpan = 12,
  disabled,
  rows = 4
}: FormTextareaProps<T>) {
  const { register, formState: { errors } } = useFormContext<T>();
  const fieldError = errors[name] as FieldError | undefined;
  const errorMessage = fieldError?.message;

  return (
    <div className={`${styles.field} ${styles[`colSpan${colSpan}`]}`}>
      <label htmlFor={String(name)}>
        {label}
        {validation?.required && <span className={styles.required}>*</span>}
      </label>
      <textarea
        id={String(name)}
        placeholder={placeholder}
        disabled={disabled}
        rows={rows}
        className={errorMessage ? styles.error : ''}
        {...register(name, validation)}
      />
      {errorMessage && (
        <span className={styles.errorMsg}>
          <i className="fas fa-exclamation-circle"></i> {errorMessage}
        </span>
      )}
    </div>
  );
}

// ============ FORM CHECKBOX ============

interface FormCheckboxProps<T extends FieldValues> {
  name: Path<T>;
  label: string;
  validation?: RegisterOptions<T, Path<T>>;
  disabled?: boolean;
}

export function FormCheckbox<T extends FieldValues>({
  name,
  label,
  validation,
  disabled
}: FormCheckboxProps<T>) {
  const { register, formState: { errors } } = useFormContext<T>();
  const fieldError = errors[name] as FieldError | undefined;
  const errorMessage = fieldError?.message;

  return (
    <div className={`${styles.field} ${styles.checkboxField}`}>
      <input
        type="checkbox"
        id={String(name)}
        disabled={disabled}
        {...register(name, validation)}
      />
      <label htmlFor={String(name)}>{label}</label>
      {errorMessage && (
        <span className={styles.errorMsg}>
          <i className="fas fa-exclamation-circle"></i> {errorMessage}
        </span>
      )}
    </div>
  );
}

// ============ FORM ACTIONS (Footer Buttons) ============

interface FormActionsProps {
  children: ReactNode;
}

export function FormActions({ children }: FormActionsProps) {
  return (
    <div className={styles.actions}>
      {children}
    </div>
  );
}

// ============ FORM SUBMIT BUTTON ============

interface FormSubmitProps {
  children: ReactNode;
  isLoading?: boolean;
  loadingText?: string;
}

export function FormSubmit({ 
  children, 
  isLoading = false, 
  loadingText = "Processando..." 
}: FormSubmitProps) {
  const { formState } = useFormContext();

  return (
    <button 
      type="submit" 
      className={styles.submitBtn} 
      disabled={isLoading || formState.isSubmitting}
    >
      {isLoading || formState.isSubmitting ? (
        <span><i className="fas fa-spinner fa-spin"></i> {loadingText}</span>
      ) : (
        children
      )}
    </button>
  );
}

// Export all components
Form.Input = FormInput;
Form.Select = FormSelect;
Form.Textarea = FormTextarea;
Form.Checkbox = FormCheckbox;
Form.Actions = FormActions;
Form.Submit = FormSubmit;