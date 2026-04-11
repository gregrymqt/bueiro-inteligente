import type { ReactNode } from 'react';
import type { UseFormReturn, FieldValues, SubmitHandler, RegisterOptions } from 'react-hook-form';

export interface FormProps<T extends FieldValues> {
  methods: UseFormReturn<T>;
  onSubmit: SubmitHandler<T>;
  children: ReactNode;
  className?: string;
}

export interface BaseFieldProps {
  name: string;
  label?: string;
  validation?: RegisterOptions;
  className?: string;
  colSpan?: 3 | 4 | 6 | 12;
}

export interface FormInputProps extends BaseFieldProps, Omit<React.InputHTMLAttributes<HTMLInputElement>, 'name' | 'className'> {}

export interface FormSelectProps extends BaseFieldProps, Omit<React.SelectHTMLAttributes<HTMLSelectElement>, 'name' | 'className'> {
  options: { label: string; value: string | number }[];
}

export interface FormTextareaProps extends BaseFieldProps, Omit<React.TextareaHTMLAttributes<HTMLTextAreaElement>, 'name' | 'className'> {}

export interface FormCheckboxProps extends BaseFieldProps, Omit<React.InputHTMLAttributes<HTMLInputElement>, 'name' | 'className' | 'type'> {}

export interface FormActionsProps {
  children: ReactNode;
  className?: string;
}

export interface FormSubmitProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  isLoading?: boolean;
}
