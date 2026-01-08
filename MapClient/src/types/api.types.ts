export interface Coordinate {
  latitude: number;
  longitude: number;
}

export interface Polygon {
  id: string;
  coordinates: Coordinate[];
}

export interface MapObject {
  id: string;
  location: Coordinate;
  objectType: string;
}

export interface CreatePolygonRequest {
  coordinates: Coordinate[];
}

export interface CreateMapObjectRequest {
  location: Coordinate;
  objectType: string;
}

export interface BatchCreateMapObjectsRequest {
  objects: CreateMapObjectRequest[];
}

export interface ApiErrorResponse {
  title: string;
  status: number;
  detail: string;
}
