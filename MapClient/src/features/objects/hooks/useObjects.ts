import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { objectApi } from '@/api/objects';
import { useToastActions } from '@/components/ui/Toast';
import { getErrorMessage } from '@/utils/errors';
import type { CreateMapObjectRequest, BatchCreateMapObjectsRequest } from '@/types/api.types';

export const OBJECTS_QUERY_KEY = ['objects'] as const;

export function useObjects() {
  const toast = useToastActions();

  return useQuery({
    queryKey: OBJECTS_QUERY_KEY,
    queryFn: async () => {
      try {
        return await objectApi.getAll();
      } catch (error) {
        toast.error(getErrorMessage(error, 'Failed to load objects'));
        throw error;
      }
    },
  });
}

interface CreateObjectOptions {
  onSuccess?: () => void;
}

export function useCreateObject(options?: CreateObjectOptions) {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (data: CreateMapObjectRequest) => objectApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
      toast.success('Object created');
      options?.onSuccess?.();
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to create object'));
    },
  });
}

interface CreateObjectsBatchOptions {
  onSuccess?: () => void;
}

export function useCreateObjectsBatch(options?: CreateObjectsBatchOptions) {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (data: BatchCreateMapObjectsRequest) => objectApi.createBatch(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
      const count = data?.length ?? 0;
      toast.success(`${count} object${count === 1 ? '' : 's'} created`);
      options?.onSuccess?.();
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to create objects'));
    },
  });
}

interface DeleteObjectOptions {
  onSuccess?: () => void;
}

export function useDeleteObject(options?: DeleteObjectOptions) {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (id: string) => objectApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: OBJECTS_QUERY_KEY });
      toast.success('Object deleted');
      options?.onSuccess?.();
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to delete object'));
    },
  });
}
