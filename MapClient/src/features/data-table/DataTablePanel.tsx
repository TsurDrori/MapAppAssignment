import { useMemo } from 'react';
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
  const { data: polygons, isLoading: polygonsLoading } = usePolygons();
  const { data: objects, isLoading: objectsLoading } = useObjects();
  const { selectedPolygonId, selectedObjectId } = useMapState();
  const dispatch = useMapDispatch();

  const isLoading = polygonsLoading || objectsLoading;

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

  const selectedKey =
    selectedPolygonId ?? selectedObjectId ?? null;

  const handleRowClick = (item: TableItem) => {
    if (item.type === 'polygon') {
      dispatch({ type: 'SELECT_POLYGON', payload: item.id });
    } else {
      dispatch({ type: 'SELECT_OBJECT', payload: item.id });
    }
  };

  const columns = [
    {
      key: 'objectType',
      header: 'Type',
      width: '30%',
      render: (item: TableItem) => (
        <span
          className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${
            item.type === 'polygon'
              ? 'bg-blue-100 text-blue-800'
              : 'bg-green-100 text-green-800'
          }`}
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
    <Panel title="Map Data" className="flex-1 flex flex-col min-h-0">
      {isLoading ? (
        <div className="flex items-center justify-center py-8 text-gray-500 text-sm">
          Loading...
        </div>
      ) : (
        <Table
          columns={columns}
          data={tableData}
          keyExtractor={(item) => item.id}
          onRowClick={handleRowClick}
          selectedKey={selectedKey}
          emptyMessage="No map data yet. Add polygons or objects to see them here."
        />
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
