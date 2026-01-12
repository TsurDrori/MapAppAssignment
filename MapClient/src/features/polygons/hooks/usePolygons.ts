import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { polygonApi } from '@/api/polygons';
import { useToastActions } from '@/components/ui/Toast';
import { getErrorMessage } from '@/utils/errors';
import type { CreatePolygonRequest } from '@/types/api.types';

export const POLYGONS_QUERY_KEY = ['polygons'] as const;

export function usePolygons() {
  return useQuery({
    queryKey: POLYGONS_QUERY_KEY,
    queryFn: polygonApi.getAll,
  });
}

export function useCreatePolygon() {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (data: CreatePolygonRequest) => polygonApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
      toast.success('Polygon created');
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to create polygon'));
    },
  });
}

export function useDeletePolygon() {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: (id: string) => polygonApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
      toast.success('Polygon deleted');
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to delete polygon'));
    },
  });
}

export function useDeleteAllPolygons() {
  const queryClient = useQueryClient();
  const toast = useToastActions();

  return useMutation({
    mutationFn: () => polygonApi.deleteAll(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
      toast.success('All polygons deleted');
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, 'Failed to delete polygons'));
    },
  });
}
