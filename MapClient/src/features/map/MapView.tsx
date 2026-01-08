import { MapContainer, TileLayer } from 'react-leaflet';
import { MAP_CONFIG } from '@/config/constants';
import { PolygonLayer } from './PolygonLayer';
import { ObjectLayer } from './ObjectLayer';
import { DrawingLayer } from './DrawingLayer';
import { MapEventHandler } from './MapEventHandler';

export function MapView() {
  return (
    <MapContainer
      center={MAP_CONFIG.defaultCenter}
      zoom={MAP_CONFIG.defaultZoom}
      className="h-full w-full"
    >
      <TileLayer
        attribution={MAP_CONFIG.tileLayerAttribution}
        url={MAP_CONFIG.tileLayerUrl}
      />
      <MapEventHandler />
      <PolygonLayer />
      <ObjectLayer />
      <DrawingLayer />
    </MapContainer>
  );
}
