import { Polyline, CircleMarker } from 'react-leaflet';
import { useMapState } from '@/context/MapContext';

export function DrawingLayer() {
  const { mode, pendingPolygonPoints, isPolygonClosed } = useMapState();

  if (mode !== 'draw-polygon' || pendingPolygonPoints.length === 0) {
    return null;
  }

  const positions = pendingPolygonPoints.map(
    (coord) => [coord.latitude, coord.longitude] as [number, number]
  );

  // If polygon is closed, connect last point to first
  const linePositions = isPolygonClosed
    ? [...positions, positions[0]]
    : positions;

  return (
    <>
      {/* Drawing line */}
      <Polyline
        positions={linePositions}
        pathOptions={{
          color: '#ef4444',
          weight: 2,
          dashArray: isPolygonClosed ? undefined : '5, 10',
        }}
      />

      {/* Vertex markers */}
      {pendingPolygonPoints.map((coord, index) => (
        <CircleMarker
          key={index}
          center={[coord.latitude, coord.longitude]}
          radius={index === 0 ? 8 : 5}
          pathOptions={{
            color: index === 0 ? '#22c55e' : '#ef4444',
            fillColor: index === 0 ? '#22c55e' : '#ef4444',
            fillOpacity: 0.8,
          }}
        />
      ))}
    </>
  );
}
