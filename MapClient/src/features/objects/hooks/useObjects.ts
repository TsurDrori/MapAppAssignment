import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { objectApi } from '@/api/objects';
import type { CreateMapObjectRequest, BatchCreateMapObjectsRequest } from '@/types/api.types';

export const OBJECTS_QUERY_KEY = ['objects'] as const;

export function useObjects() {
  return useQuery({
    queryKey: OBJECTS_QUERY_KEY,
    queryFn: objectApi.getAll,
  });
}

export function useCreateObject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateMapObjectRequest) => objectApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
    },
  });
}

export function useCreateObjectsBatch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BatchCreateMapObjectsRequest) => objectApi.createBatch(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
    },
  });
}

export function useDeleteObject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => objectApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
    },
  });
}

export function useDeleteAllObjects() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => objectApi.deleteAll(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
    },
  });
}
