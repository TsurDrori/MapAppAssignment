export const MAP_CONFIG = {
  defaultCenter: [32.0853, 34.7818] as [number, number], // Tel Aviv
  defaultZoom: 13,
  tileLayerUrl: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
  tileLayerAttribution:
    '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
} as const;

export const CLOSE_POLYGON_THRESHOLD_PIXELS = 15;
