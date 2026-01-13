import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { polygonApi } from '@/api/polygons';
import { useToastActions } from '@/components/ui/Toast';
import { getErrorMessage } from '@/utils/errors';
import type { CreatePolygonRequest } from '@/types/api.types';

export const POLYGONS_QUERY_KEY = ['polygons'] as const;

export function usePolygons() {
  const toast = useToastActions();

  return useQuery({
    queryKey: POLYGONS_QUERY_KEY,
    queryFn: async () => {
      try {
        return await polygonApi.getAll();
      } catch (error) {
        toast.error(getErrorMessage(error, 'Failed to load polygons'));
        throw error;
      }
    },
  });
}

interface CreatePolygonOptions {
  onSuccess?: () => void;
}

export function useCreatePolygon(options?: CreatePolygonOptions) {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (data: CreatePolygonRequest) => polygonApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
      toast.success('Polygon created');
      options?.onSuccess?.();
    },
    // Note: onError not defined - PolygonPanel handles errors inline
  });
}

interface DeletePolygonOptions {
  onSuccess?: () => void;
}

export function useDeletePolygon(options?: DeletePolygonOptions) {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (id: string) => polygonApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
      toast.success('Polygon deleted');
      options?.onSuccess?.();
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to delete polygon'));
    },
  });
}
