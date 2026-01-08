import { ErrorBoundary } from '@/components/ErrorBoundary';
import { MapView } from '@/features/map/MapView';
import { PolygonPanel } from '@/features/polygons/PolygonPanel';
import { ObjectPanel } from '@/features/objects/ObjectPanel';
import { DataTablePanel } from '@/features/data-table/DataTablePanel';
import { useMapState } from '@/context/MapContext';

function AppContent() {
  const { mode } = useMapState();

  return (
    <div className="h-screen flex flex-col bg-gray-100">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Map Application</h1>
        {mode !== 'view' && (
          <span className="text-sm text-blue-600 bg-blue-50 px-3 py-1 rounded-full">
            {mode === 'draw-polygon' ? 'Drawing Polygon' : 'Placing Object'}
          </span>
        )}
      </header>

      {/* Main content */}
      <div className="flex-1 flex min-h-0">
        {/* Map panel - takes remaining space */}
        <main className="flex-1 min-w-0">
          <MapView />
        </main>

        {/* Side panels */}
        <aside className="w-80 flex flex-col bg-white border-l border-gray-200 overflow-hidden">
          <PolygonPanel />
          <ObjectPanel />
          <DataTablePanel />
        </aside>
      </div>
    </div>
  );
}

export default function App() {
  return (
    <ErrorBoundary>
      <AppContent />
    </ErrorBoundary>
  );
}
