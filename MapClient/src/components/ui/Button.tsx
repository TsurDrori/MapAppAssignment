import { memo, type ButtonHTMLAttributes, type ReactNode } from 'react';

type ButtonVariant = 'primary' | 'secondary' | 'danger';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  children: ReactNode;
  isLoading?: boolean;
}

const variantStyles: Record<ButtonVariant, string> = {
  primary:
    'bg-sky-600 text-white ring-sky-600/25 hover:bg-sky-700 active:bg-sky-800',
  secondary:
    'bg-white/80 text-slate-900 ring-slate-900/10 hover:bg-white hover:ring-slate-900/15 active:bg-slate-50',
  danger:
    'bg-rose-600 text-white ring-rose-600/25 hover:bg-rose-700 active:bg-rose-800',
};

export const Button = memo(function Button({
  variant = 'primary',
  children,
  isLoading = false,
  disabled,
  className = '',
  ...props
}: ButtonProps) {
  return (
    <button
      className={[
        'inline-flex items-center justify-center gap-2',
        'rounded-lg px-3.5 py-2 text-sm font-semibold',
        'shadow-sm ring-1 ring-inset',
        'transition',
        'hover:-translate-y-px active:translate-y-0',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sky-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white',
        'disabled:pointer-events-none disabled:opacity-55 disabled:shadow-none',
        variantStyles[variant],
        className,
      ].join(' ')}
      disabled={disabled || isLoading}
      aria-busy={isLoading || undefined}
      {...props}
    >
      {isLoading ? (
        <span className="flex items-center gap-2">
          <svg
            className="animate-spin h-4 w-4"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          <span className="leading-none">Loading…</span>
        </span>
      ) : (
        children
      )}
    </button>
  );
});
