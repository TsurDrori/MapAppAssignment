import type { ReactNode } from 'react';

interface Column<T> {
  key: string;
  header: string;
  render: (item: T) => ReactNode;
  width?: string;
}

interface TableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyExtractor: (item: T) => string;
  onRowClick?: (item: T) => void;
  selectedKey?: string | null;
  emptyMessage?: string;
  scrollAreaClassName?: string;
  className?: string;
}

export function Table<T>({
  columns,
  data,
  keyExtractor,
  onRowClick,
  selectedKey,
  emptyMessage = 'No data available',
  scrollAreaClassName = 'max-h-72',
  className = '',
}: TableProps<T>) {
  if (data.length === 0) {
    return (
      <div className="rounded-xl bg-white/60 ring-1 ring-inset ring-slate-900/10 p-6 text-center">
        <div className="mx-auto mb-3 flex h-10 w-10 items-center justify-center rounded-full bg-slate-900/5 ring-1 ring-inset ring-slate-900/10">
          <svg
            viewBox="0 0 24 24"
            fill="none"
            className="h-5 w-5 text-slate-600"
            aria-hidden="true"
          >
            <path
              d="M7 7h10M7 11h10M7 15h6M6 3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3Z"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
            />
          </svg>
        </div>
        <p className="text-sm text-slate-600">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div
      className={[
        'overflow-hidden rounded-xl bg-white/60 ring-1 ring-inset ring-slate-900/10',
        className,
      ].join(' ')}
    >
      <div
        className={[
          scrollAreaClassName,
          'overflow-auto scrollbar-thin scrollbar-thumb-rounded',
        ].join(' ')}
      >
        <table className="w-full text-sm">
          <thead className="sticky top-0 bg-white/70 backdrop-blur border-b border-slate-200/60">
            <tr>
              {columns.map((col) => (
                <th
                  key={col.key}
                  className="px-3 py-2 text-left text-xs font-semibold tracking-wide text-slate-600"
                  style={{ width: col.width }}
                >
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200/60">
            {data.map((item) => {
              const key = keyExtractor(item);
              const isSelected = selectedKey === key;
              return (
                <tr
                  key={key}
                  onClick={() => onRowClick?.(item)}
                  className={[
                    onRowClick ? 'cursor-pointer hover:bg-slate-50/70' : '',
                    isSelected ? 'bg-sky-500/10' : '',
                  ].join(' ')}
                >
                  {columns.map((col) => (
                    <td key={col.key} className="px-3 py-2 text-slate-700">
                      {col.render(item)}
                    </td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
