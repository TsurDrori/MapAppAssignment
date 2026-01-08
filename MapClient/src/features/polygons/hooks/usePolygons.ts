import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { polygonApi } from '@/api/polygons';
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

  return useMutation({
    mutationFn: (data: CreatePolygonRequest) => polygonApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
    },
  });
}

export function useDeletePolygon() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => polygonApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
    },
  });
}

export function useDeleteAllPolygons() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => polygonApi.deleteAll(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: POLYGONS_QUERY_KEY });
    },
  });
}
