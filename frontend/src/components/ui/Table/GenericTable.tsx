import type { ReactNode } from 'react';
import styles from './GenericTable.module.scss';
import type { GenericTableProps } from './types/index.types';

export function GenericTable<T extends { id: string | number }>({ 
  data, 
  columns, 
  isLoading,
  emptyMessage = "Nenhum registro encontrado." 
}: GenericTableProps<T>) {

  if (data.length === 0 && !isLoading) {
    return <div className={styles.empty}>{emptyMessage}</div>;
  }

  return (
    <div className={styles.container}>
      <table className={styles.table}>
        <thead className={styles.thead}>
          <tr>
            {columns.map((col) => (
              <th key={col.label} className={styles.th}>
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className={styles.tbody}>
          {data.map((item) => (
            <tr key={item.id} className={styles.row}>
              {columns.map((col) => {
                const cellValue = item[col.key as keyof T];
                const cellContent: ReactNode = col.render
                  ? col.render(cellValue, item)
                  : (cellValue as ReactNode);

                return (
                  <td 
                    key={String(col.key)} 
                    className={styles.cell} 
                    data-label={col.label} // Vital para UX mobile
                  >
                    <div className={styles.cellContent}>
                      {cellContent}
                    </div>
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}