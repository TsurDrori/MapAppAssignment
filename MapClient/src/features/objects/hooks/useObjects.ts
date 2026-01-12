import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { objectApi } from '@/api/objects';
import { useToastActions } from '@/components/ui/Toast';
import { getErrorMessage } from '@/utils/errors';
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
  const toast = useToastActions();

  return useMutation({
    mutationFn: (data: CreateMapObjectRequest) => objectApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
      toast.success('Object created');
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to create object'));
    },
  });
}

export function useCreateObjectsBatch() {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (data: BatchCreateMapObjectsRequest) => objectApi.createBatch(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
      const count = data?.length ?? 0;
      toast.success(`${count} object${count === 1 ? '' : 's'} created`);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to create objects'));
    },
  });
}

export function useDeleteObject() {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (id: string) => objectApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
      toast.success('Object deleted');
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to delete object'));
    },
  });
}

export function useDeleteAllObjects() {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: () => objectApi.deleteAll(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
      toast.success('All objects deleted');
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to delete objects'));
    },
  });
}
