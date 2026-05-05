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
              {columns.map((col) => (
                <td 
                  key={String(col.key)} 
                  className={styles.cell} 
                  data-label={col.label} // Vital para UX mobile
                >
                  <div className={styles.cellContent}>
                    {col.render 
                      ? col.render((item as any)[col.key], item)
                      : (item as any)[col.key]
                    }
                  </div>
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}