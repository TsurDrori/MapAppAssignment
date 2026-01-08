import { useMemo } from 'react';
import { Marker, Tooltip } from 'react-leaflet';
import L from 'leaflet';
import { useObjects } from '@/features/objects/hooks/useObjects';
import { useMapState, useMapDispatch } from '@/context/MapContext';
import { getMarkerConfig } from '@/config/markerTypes';

export function ObjectLayer() {
  const { data: objects } = useObjects();
  const { selectedObjectId, pendingObjects } = useMapState();
  const dispatch = useMapDispatch();

  // Create icons for each marker type (memoized)
  const iconCache = useMemo(() => {
    const cache = new Map<string, L.Icon>();
    return {
      get: (typeId: string) => {
        if (!cache.has(typeId)) {
          const config = getMarkerConfig(typeId);
          cache.set(
            typeId,
            new L.Icon({
              iconUrl: config.iconUrl,
              iconSize: config.iconSize,
              iconAnchor: config.iconAnchor,
            })
          );
        }
        return cache.get(typeId)!;
      },
    };
  }, []);

  return (
    <>
      {/* Saved objects from server */}
      {objects?.map((obj) => {
        const isSelected = selectedObjectId === obj.id;
        return (
          <Marker
            key={obj.id}
            position={[obj.location.latitude, obj.location.longitude]}
            icon={iconCache.get(obj.objectType)}
            eventHandlers={{
              click: (e) => {
                e.originalEvent.stopPropagation();
                dispatch({ type: 'SELECT_OBJECT', payload: obj.id });
              },
            }}
            opacity={isSelected ? 1 : 0.8}
          >
            <Tooltip>
              {obj.objectType} ({obj.location.latitude.toFixed(4)},{' '}
              {obj.location.longitude.toFixed(4)})
            </Tooltip>
          </Marker>
        );
      })}

      {/* Pending objects (not yet saved) */}
      {pendingObjects.map((obj, index) => (
        <Marker
          key={`pending-${index}`}
          position={[obj.location.latitude, obj.location.longitude]}
          icon={iconCache.get(obj.objectType)}
          opacity={0.6}
        >
          <Tooltip>
            {obj.objectType} (pending) - ({obj.location.latitude.toFixed(4)},{' '}
            {obj.location.longitude.toFixed(4)})
          </Tooltip>
        </Marker>
      ))}
    </>
  );
}
