import { api } from './client';
import type {
  MapObject,
  CreateMapObjectRequest,
  BatchCreateMapObjectsRequest,
} from '@/types/api.types';

export const objectApi = {
  getAll: () => api.get<MapObject[]>('/api/objects'),

  getById: (id: string) => api.get<MapObject>(`/api/objects/${id}`),

  create: (data: CreateMapObjectRequest) => api.post<MapObject>('/api/objects', data),

  createBatch: (data: BatchCreateMapObjectsRequest) =>
    api.post<MapObject[]>('/api/objects/batch', data),

  delete: (id: string) => api.delete(`/api/objects/${id}`),

  deleteAll: () => api.delete('/api/objects'),
};
