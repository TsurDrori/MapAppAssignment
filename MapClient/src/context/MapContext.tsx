import {
  createContext,
  useContext,
  useReducer,
  type ReactNode,
  type Dispatch,
} from 'react';
import type { Coordinate } from '@/types/api.types';
import { getDefaultMarkerType } from '@/config/markerTypes';

// State types
export type MapMode = 'view' | 'draw-polygon' | 'place-object';

interface MapState {
  mode: MapMode;
  pendingPolygonPoints: Coordinate[];
  isPolygonClosed: boolean;
  selectedPolygonId: string | null;
  selectedObjectId: string | null;
  objectTypeToPlace: string;
  pendingObjects: Array<{ location: Coordinate; objectType: string }>;
}

// Action types
type MapAction =
  | { type: 'ENTER_DRAW_MODE' }
  | { type: 'EXIT_DRAW_MODE' }
  | { type: 'ADD_POLYGON_POINT'; payload: Coordinate }
  | { type: 'CLOSE_POLYGON' }
  | { type: 'CLEAR_PENDING_POLYGON' }
  | { type: 'ENTER_PLACE_OBJECT_MODE'; payload: string }
  | { type: 'EXIT_PLACE_OBJECT_MODE' }
  | { type: 'ADD_PENDING_OBJECT'; payload: { location: Coordinate; objectType: string } }
  | { type: 'CLEAR_PENDING_OBJECTS' }
  | { type: 'SELECT_POLYGON'; payload: string | null }
  | { type: 'SELECT_OBJECT'; payload: string | null }
  | { type: 'SET_OBJECT_TYPE'; payload: string };

// Initial state
const initialState: MapState = {
  mode: 'view',
  pendingPolygonPoints: [],
  isPolygonClosed: false,
  selectedPolygonId: null,
  selectedObjectId: null,
  objectTypeToPlace: getDefaultMarkerType(),
  pendingObjects: [],
};

// Reducer
function mapReducer(state: MapState, action: MapAction): MapState {
  switch (action.type) {
    case 'ENTER_DRAW_MODE':
      return {
        ...state,
        mode: 'draw-polygon',
        pendingPolygonPoints: [],
        isPolygonClosed: false,
        selectedPolygonId: null,
        selectedObjectId: null,
      };

    case 'EXIT_DRAW_MODE':
      return {
        ...state,
        mode: 'view',
        pendingPolygonPoints: [],
        isPolygonClosed: false,
      };

    case 'ADD_POLYGON_POINT':
      return {
        ...state,
        pendingPolygonPoints: [...state.pendingPolygonPoints, action.payload],
      };

    case 'CLOSE_POLYGON':
      return {
        ...state,
        isPolygonClosed: true,
      };

    case 'CLEAR_PENDING_POLYGON':
      return {
        ...state,
        pendingPolygonPoints: [],
        isPolygonClosed: false,
      };

    case 'ENTER_PLACE_OBJECT_MODE':
      return {
        ...state,
        mode: 'place-object',
        objectTypeToPlace: action.payload,
        selectedPolygonId: null,
        selectedObjectId: null,
      };

    case 'EXIT_PLACE_OBJECT_MODE':
      return {
        ...state,
        mode: 'view',
      };

    case 'ADD_PENDING_OBJECT':
      return {
        ...state,
        pendingObjects: [...state.pendingObjects, action.payload],
      };

    case 'CLEAR_PENDING_OBJECTS':
      return {
        ...state,
        pendingObjects: [],
      };

    case 'SELECT_POLYGON':
      return {
        ...state,
        selectedPolygonId: action.payload,
        selectedObjectId: null,
      };

    case 'SELECT_OBJECT':
      return {
        ...state,
        selectedObjectId: action.payload,
        selectedPolygonId: null,
      };

    case 'SET_OBJECT_TYPE':
      return {
        ...state,
        objectTypeToPlace: action.payload,
      };

    default:
      return state;
  }
}

// Split contexts to prevent unnecessary re-renders
const MapStateContext = createContext<MapState | null>(null);
const MapDispatchContext = createContext<Dispatch<MapAction> | null>(null);

// Provider component
export function MapProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(mapReducer, initialState);

  return (
    <MapStateContext.Provider value={state}>
      <MapDispatchContext.Provider value={dispatch}>
        {children}
      </MapDispatchContext.Provider>
    </MapStateContext.Provider>
  );
}

// Custom hooks for consuming context
export function useMapState(): MapState {
  const context = useContext(MapStateContext);
  if (context === null) {
    throw new Error('useMapState must be used within a MapProvider');
  }
  return context;
}

export function useMapDispatch(): Dispatch<MapAction> {
  const context = useContext(MapDispatchContext);
  if (context === null) {
    throw new Error('useMapDispatch must be used within a MapProvider');
  }
  return context;
}

// Convenience hook that returns both (use sparingly to avoid re-renders)
export function useMap() {
  return {
    state: useMapState(),
    dispatch: useMapDispatch(),
  };
}
