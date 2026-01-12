import { MARKER_TYPES } from '@/config/markerTypes';

interface ObjectTypeSelectorProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

export function ObjectTypeSelector({
  value,
  onChange,
  disabled = false,
}: ObjectTypeSelectorProps) {
  return (
    <div className="flex items-center justify-between gap-3">
      <label
        htmlFor="object-type"
        className="text-xs font-semibold tracking-wide text-slate-600"
      >
        Object type
      </label>

      <div className="relative min-w-0 flex-1">
        <select
          id="object-type"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          className={[
            'w-full appearance-none',
            'rounded-lg bg-white/80 px-3 py-2 text-sm text-slate-900',
            'shadow-sm ring-1 ring-inset ring-slate-900/10',
            'focus:outline-none focus:ring-2 focus:ring-sky-500 focus:ring-offset-2 focus:ring-offset-white',
            'disabled:opacity-60',
          ].join(' ')}
        >
          {MARKER_TYPES.map((type) => (
            <option key={type.id} value={type.id}>
              {type.label}
            </option>
          ))}
        </select>
        <svg
          viewBox="0 0 20 20"
          fill="currentColor"
          className="pointer-events-none absolute right-2 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-500"
          aria-hidden="true"
        >
          <path
            fillRule="evenodd"
            d="M5.23 7.21a.75.75 0 0 1 1.06.02L10 11.168l3.71-3.938a.75.75 0 1 1 1.08 1.04l-4.24 4.5a.75.75 0 0 1-1.08 0l-4.24-4.5a.75.75 0 0 1 .02-1.06Z"
            clipRule="evenodd"
          />
        </svg>
      </div>
    </div>
  );
}
