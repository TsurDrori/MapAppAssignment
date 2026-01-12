export class ApiError extends Error {
  status: number;
  detail: string;

  constructor(status: number, detail: string) {
    super(detail);
    this.name = 'ApiError';
    this.status = status;
    this.detail = detail;
  }
}

/**
 * Maps HTTP status codes to user-friendly messages.
 */
function getHttpErrorMessage(status: number, serverDetail?: string): string {
  if (serverDetail) return serverDetail;

  switch (status) {
    case 400: return 'Invalid request data';
    case 401: return 'Authentication required';
    case 403: return 'Access denied';
    case 404: return 'Resource not found';
    case 409: return 'Conflict with existing data';
    case 422: return 'Validation failed';
    case 429: return 'Too many requests, please slow down';
    case 500: return 'Server error, please try again';
    case 502: return 'Server is temporarily unavailable';
    case 503: return 'Service unavailable, please try again later';
    default: return `Request failed (${status})`;
  }
}

async function request<T>(endpoint: string, options?: RequestInit): Promise<T> {
  let response: Response;

  try {
    response = await fetch(endpoint, {
      headers: {
        'Content-Type': 'application/json',
      },
      ...options,
    });
  } catch (error) {
    // Network error - server unreachable, no internet, CORS, etc.
    if (error instanceof TypeError) {
      throw new ApiError(0, 'Cannot connect to server. Please check your connection.');
    }
    throw new ApiError(0, 'Network error occurred');
  }

  if (!response.ok) {
    const errorBody = await response.json().catch(() => ({}));
    const detail = getHttpErrorMessage(response.status, errorBody.detail);
    throw new ApiError(response.status, detail);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

export const api = {
  get: <T>(endpoint: string) => request<T>(endpoint),
  post: <T>(endpoint: string, data: unknown) =>
    request<T>(endpoint, { method: 'POST', body: JSON.stringify(data) }),
  delete: (endpoint: string) => request<void>(endpoint, { method: 'DELETE' }),
};
