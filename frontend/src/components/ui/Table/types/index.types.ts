import type { ReactNode } from 'react';

export interface Column<T> {
  key: keyof T | string;
  label: string;
  // Permite customizar a renderização (ex: badges, botões)
  render?: (value: any, item: T) => ReactNode;
  // Esconde em telas muito pequenas se necessário
  priority?: 'high' | 'low'; 
}

export interface GenericTableProps<T> {
  data: T[];
  columns: Column<T>[];
  isLoading?: boolean;
  emptyMessage?: string;
}