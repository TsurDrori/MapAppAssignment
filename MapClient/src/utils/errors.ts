import { ApiError } from '@/api/client';

/**
 * Extracts a user-friendly error message from an error object.
 * Handles ApiError instances and generic errors.
 */
export function getErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiError) {
    return error.detail;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return fallback;
}
