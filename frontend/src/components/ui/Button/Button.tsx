import type { ReactNode, ButtonHTMLAttributes } from 'react';
import styles from './Button.module.scss';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
}

export const Button = ({
  children,
  variant = 'primary',
  size = 'md',
  isLoading = false,
  leftIcon,
  rightIcon,
  className = '',
  disabled,
  ...props
}: ButtonProps) => {
  const rootClass = [
    styles.btn,
    styles[variant],
    styles[size],
    isLoading ? styles.loading : '',
    className
  ].filter(Boolean).join(' ');

  return (
    <button className={rootClass} disabled={disabled || isLoading} {...props}>
      {isLoading && <span className={styles.spinner}><i className="fas fa-spinner fa-spin"></i></span>}
      {!isLoading && leftIcon && <span className={styles.icon}>{leftIcon}</span>}
      <span className={styles.content}>{children}</span>
      {!isLoading && rightIcon && <span className={styles.icon}>{rightIcon}</span>}
    </button>
  );
};
