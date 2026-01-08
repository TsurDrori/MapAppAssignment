import { api } from './client';
import type { Polygon, CreatePolygonRequest } from '@/types/api.types';

export const polygonApi = {
  getAll: () => api.get<Polygon[]>('/api/polygons'),

  getById: (id: string) => api.get<Polygon>(`/api/polygons/${id}`),

  create: (data: CreatePolygonRequest) => api.post<Polygon>('/api/polygons', data),

  delete: (id: string) => api.delete(`/api/polygons/${id}`),

  deleteAll: () => api.delete('/api/polygons'),
};
