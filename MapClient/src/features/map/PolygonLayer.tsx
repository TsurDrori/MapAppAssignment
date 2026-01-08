import { Polygon, Tooltip } from 'react-leaflet';
import { usePolygons } from '@/features/polygons/hooks/usePolygons';
import { useMapState, useMapDispatch } from '@/context/MapContext';

export function PolygonLayer() {
  const { data: polygons } = usePolygons();
  const { selectedPolygonId } = useMapState();
  const dispatch = useMapDispatch();

  if (!polygons) return null;

  return (
    <>
      {polygons.map((polygon) => {
        const positions = polygon.coordinates.map(
          (coord) => [coord.latitude, coord.longitude] as [number, number]
        );
        const isSelected = selectedPolygonId === polygon.id;

        return (
          <Polygon
            key={polygon.id}
            positions={positions}
            pathOptions={{
              color: isSelected ? '#2563eb' : '#3b82f6',
              fillColor: isSelected ? '#3b82f6' : '#93c5fd',
              fillOpacity: isSelected ? 0.4 : 0.2,
              weight: isSelected ? 3 : 2,
            }}
            eventHandlers={{
              click: (e) => {
                e.originalEvent.stopPropagation();
                dispatch({ type: 'SELECT_POLYGON', payload: polygon.id });
              },
            }}
          >
            <Tooltip>Polygon {polygon.id.slice(-6)}</Tooltip>
          </Polygon>
        );
      })}
    </>
  );
}
