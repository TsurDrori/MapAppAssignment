import type { Coordinate } from '@/types/api.types';

export interface ValidationResult {
  valid: boolean;
  error?: string;
}

/**
 * Validates a single coordinate against geographic bounds.
 * Latitude: -90 to 90
 * Longitude: -180 to 180
 */
export function validateCoordinate(coord: Coordinate): ValidationResult {
  if (typeof coord.latitude !== 'number' || isNaN(coord.latitude)) {
    return { valid: false, error: 'Latitude must be a number' };
  }
  if (typeof coord.longitude !== 'number' || isNaN(coord.longitude)) {
    return { valid: false, error: 'Longitude must be a number' };
  }
  if (coord.latitude < -90 || coord.latitude > 90) {
    return { valid: false, error: `Latitude ${coord.latitude} is out of range (-90 to 90)` };
  }
  if (coord.longitude < -180 || coord.longitude > 180) {
    return { valid: false, error: `Longitude ${coord.longitude} is out of range (-180 to 180)` };
  }
  return { valid: true };
}

/**
 * Validates an array of coordinates (e.g., for a polygon).
 */
export function validateCoordinates(coords: Coordinate[]): ValidationResult {
  if (!Array.isArray(coords) || coords.length === 0) {
    return { valid: false, error: 'At least one coordinate is required' };
  }

  for (let i = 0; i < coords.length; i++) {
    const result = validateCoordinate(coords[i]);
    if (!result.valid) {
      return { valid: false, error: `Point ${i + 1}: ${result.error}` };
    }
  }

  return { valid: true };
}

/**
 * Validates polygon-specific requirements.
 */
export function validatePolygon(coords: Coordinate[]): ValidationResult {
  const coordsResult = validateCoordinates(coords);
  if (!coordsResult.valid) {
    return coordsResult;
  }

  if (coords.length < 3) {
    return { valid: false, error: 'Polygon requires at least 3 points' };
  }

  return { valid: true };
}
