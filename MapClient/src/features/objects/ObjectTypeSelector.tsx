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
    <div className="flex items-center gap-2">
      <label htmlFor="object-type" className="text-sm text-gray-600">
        Type:
      </label>
      <select
        id="object-type"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled}
        className="px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
      >
        {MARKER_TYPES.map((type) => (
          <option key={type.id} value={type.id}>
            {type.label}
          </option>
        ))}
      </select>
    </div>
  );
}
