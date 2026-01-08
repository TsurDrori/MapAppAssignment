import { useEffect } from 'react';
import { useMapEvents } from 'react-leaflet';
import { useMapState, useMapDispatch } from '@/context/MapContext';
import { CLOSE_POLYGON_THRESHOLD_PIXELS } from '@/config/constants';

export function MapEventHandler() {
  const { mode, pendingPolygonPoints, objectTypeToPlace } = useMapState();
  const dispatch = useMapDispatch();

  const map = useMapEvents({
    click(e) {
      if (mode === 'draw-polygon') {
        const newPoint = {
          latitude: e.latlng.lat,
          longitude: e.latlng.lng,
        };

        // Check if clicking near first point to close polygon (need at least 3 points)
        if (pendingPolygonPoints.length >= 3) {
          const firstPoint = pendingPolygonPoints[0];
          const firstPointPixel = map.latLngToContainerPoint([
            firstPoint.latitude,
            firstPoint.longitude,
          ]);
          const clickPixel = e.containerPoint;

          const distance = Math.sqrt(
            Math.pow(firstPointPixel.x - clickPixel.x, 2) +
              Math.pow(firstPointPixel.y - clickPixel.y, 2)
          );

          if (distance < CLOSE_POLYGON_THRESHOLD_PIXELS) {
            dispatch({ type: 'CLOSE_POLYGON' });
            return;
          }
        }

        dispatch({ type: 'ADD_POLYGON_POINT', payload: newPoint });
      } else if (mode === 'place-object') {
        dispatch({
          type: 'ADD_PENDING_OBJECT',
          payload: {
            location: {
              latitude: e.latlng.lat,
              longitude: e.latlng.lng,
            },
            objectType: objectTypeToPlace,
          },
        });
      }
    },
  });

  // Handle Escape key to cancel current operation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (mode === 'draw-polygon') {
          dispatch({ type: 'EXIT_DRAW_MODE' });
        } else if (mode === 'place-object') {
          dispatch({ type: 'EXIT_PLACE_OBJECT_MODE' });
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [mode, dispatch]);

  return null;
}
