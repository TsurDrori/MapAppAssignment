export interface MarkerTypeConfig {
  id: string;
  label: string;
  iconUrl: string;
  iconSize: [number, number];
  iconAnchor: [number, number];
}

/**
 * Extensible marker types configuration.
 * To add a new marker type:
 * 1. Add the icon file to /public/icons/
 * 2. Add an entry to this array
 * No other code changes required.
 */
export const MARKER_TYPES: MarkerTypeConfig[] = [
  {
    id: 'marker',
    label: 'Marker',
    iconUrl: '/icons/marker.svg',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
  },
  {
    id: 'jeep',
    label: 'Jeep',
    iconUrl: '/icons/jeep.svg',
    iconSize: [32, 32],
    iconAnchor: [16, 16],
  },
];

export function getMarkerConfig(typeId: string): MarkerTypeConfig {
  return MARKER_TYPES.find((t) => t.id === typeId) ?? MARKER_TYPES[0];
}

export function getDefaultMarkerType(): string {
  return MARKER_TYPES[0].id;
}
