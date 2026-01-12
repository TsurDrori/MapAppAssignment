import type { ReactNode } from 'react';

interface PanelProps {
  title: string;
  description?: string;
  children: ReactNode;
  className?: string;
  contentClassName?: string;
}

export function Panel({
  title,
  description,
  children,
  className = '',
  contentClassName = '',
}: PanelProps) {
  return (
    <section
      className={[
        'rounded-2xl bg-white/70 backdrop-blur',
        'shadow-sm ring-1 ring-inset ring-slate-900/10',
        className,
      ].join(' ')}
    >
      <header className="px-3 py-2 sm:px-4 sm:py-3 border-b border-slate-200/60">
        <h2 className="text-sm font-semibold text-slate-900">{title}</h2>
        {description ? (
          <p className="mt-0.5 text-xs text-slate-500">{description}</p>
        ) : null}
      </header>
      <div className={['p-3 sm:p-4', contentClassName].join(' ')}>
        {children}
      </div>
    </section>
  );
}
