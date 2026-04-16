import type { ReactNode } from 'react';

export interface NavigationItem {
  id: string;          // Um identificador único (ex: 'monitoramento', 'historico')
  label: string;       // O nome que aparece no menu (ex: 'Visão Geral')
  icon: ReactNode;     // O ícone (pode ser um SVG ou um componente de biblioteca como Lucide/Phosphor)
  component?: ReactNode;// O componente React que será renderizado no ecrã principal
  children?: NavigationItem[];
}