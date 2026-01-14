import { useMemo, useCallback } from 'react';
import { Panel } from '@/components/ui/Panel';
import { Table } from '@/components/ui/Table';
import { usePolygons } from '@/features/polygons/hooks/usePolygons';
import { useObjects } from '@/features/objects/hooks/useObjects';
import { useMapState, useMapDispatch } from '@/context/MapContext';

interface TableItem {
  id: string;
  type: 'polygon' | 'object';
  objectType: string;
  latitude: number;
  longitude: number;
}

export function DataTablePanel() {
  const { data: polygons, isLoading: polygonsLoading, isError: polygonsError } = usePolygons();
  const { data: objects, isLoading: objectsLoading, isError: objectsError } = useObjects();
  const { selectedPolygonId, selectedObjectId } = useMapState();
  const dispatch = useMapDispatch();

  const isLoading = polygonsLoading || objectsLoading;
  const hasError = polygonsError || objectsError;

  // Combine polygons and objects into a single table
  const tableData = useMemo<TableItem[]>(() => {
    const items: TableItem[] = [];

    // Add polygons (using centroid as representative coordinate)
    polygons?.forEach((polygon) => {
      if (polygon.coordinates.length > 0) {
        const centroid = calculateCentroid(polygon.coordinates);
        items.push({
          id: polygon.id,
          type: 'polygon',
          objectType: 'Polygon',
          latitude: centroid.latitude,
          longitude: centroid.longitude,
        });
      }
    });

    // Add objects
    objects?.forEach((obj) => {
      items.push({
        id: obj.id,
        type: 'object',
        objectType: obj.objectType,
        latitude: obj.location.latitude,
        longitude: obj.location.longitude,
      });
    });

    return items;
  }, [polygons, objects]);

  const selectedKey = selectedPolygonId ?? selectedObjectId ?? null;

  const handleRowClick = useCallback((item: TableItem) => {
    if (item.type === 'polygon') {
      dispatch({ type: 'SELECT_POLYGON', payload: item.id });
    } else {
      dispatch({ type: 'SELECT_OBJECT', payload: item.id });
    }
  }, [dispatch]);

  const columns = [
    {
      key: 'objectType',
      header: 'Type',
      width: '30%',
      render: (item: TableItem) => (
        <span
          className={[
            'inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ring-inset',
            item.type === 'polygon'
              ? 'bg-sky-500/10 text-sky-700 ring-sky-600/20'
              : 'bg-emerald-500/10 text-emerald-700 ring-emerald-600/20',
          ].join(' ')}
        >
          {item.objectType}
        </span>
      ),
    },
    {
      key: 'latitude',
      header: 'Lat',
      width: '35%',
      render: (item: TableItem) => item.latitude.toFixed(5),
    },
    {
      key: 'longitude',
      header: 'Lon',
      width: '35%',
      render: (item: TableItem) => item.longitude.toFixed(5),
    },
  ];

  return (
    <Panel
      title="Map Data"
      description="Click a row to select and highlight on the map"
      className="flex-1 flex flex-col min-h-0"
      contentClassName="flex flex-1 flex-col min-h-0"
    >
      {isLoading ? (
        <div className="flex flex-1 items-center justify-center py-8 text-sm text-slate-600">
          Loading…
        </div>
      ) : hasError ? (
        <div className="flex flex-1 items-center justify-center py-8 text-sm text-rose-600">
          Failed to load data. Please try again.
        </div>
      ) : (
        <div className="flex-1 min-h-0">
          <Table
            columns={columns}
            data={tableData}
            keyExtractor={(item) => item.id}
            onRowClick={handleRowClick}
            selectedKey={selectedKey}
            emptyMessage="No map data yet. Add polygons or objects to see them here."
            className="h-full"
            scrollAreaClassName="h-full min-h-0"
          />
        </div>
      )}
    </Panel>
  );
}

// Helper function to calculate polygon centroid
function calculateCentroid(
  coordinates: Array<{ latitude: number; longitude: number }>
): { latitude: number; longitude: number } {
  const sum = coordinates.reduce(
    (acc, coord) => ({
      latitude: acc.latitude + coord.latitude,
      longitude: acc.longitude + coord.longitude,
    }),
    { latitude: 0, longitude: 0 }
  );

  return {
    latitude: sum.latitude / coordinates.length,
    longitude: sum.longitude / coordinates.length,
  };
}

// Default export for lazy loading
export default DataTablePanel;
