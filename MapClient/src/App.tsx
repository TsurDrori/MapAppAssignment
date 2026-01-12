import { ErrorBoundary } from '@/components/ErrorBoundary';
import { MapView } from '@/features/map/MapView';
import { PolygonPanel } from '@/features/polygons/PolygonPanel';
import { ObjectPanel } from '@/features/objects/ObjectPanel';
import { DataTablePanel } from '@/features/data-table/DataTablePanel';
import { useMapState } from '@/context/MapContext';

function AppContent() {
  const { mode } = useMapState();

  const statusLabel =
    mode === 'view'
      ? 'View mode'
      : mode === 'draw-polygon'
        ? 'Drawing polygon'
        : 'Placing objects';

  const statusHint =
    mode === 'view'
      ? 'Select items on the map'
      : mode === 'draw-polygon'
        ? 'Click map to add points'
        : 'Click map to place markers';

  return (
    <div className="min-h-screen flex flex-col">
      <header className="sticky top-0 z-20 border-b border-slate-200/70 bg-white/70 backdrop-blur supports-[backdrop-filter]:bg-white/60">
        <div className="mx-auto max-w-screen-2xl px-3 py-2 sm:px-4 sm:py-3 lg:px-6 flex items-center justify-between gap-4">
          <div className="flex items-center gap-3 min-w-0">
            <div className="h-9 w-9 rounded-xl bg-gradient-to-br from-sky-500 to-indigo-500 shadow-sm ring-1 ring-black/5" />
            <div className="min-w-0">
              <h1 className="text-base font-semibold text-slate-900 leading-tight truncate">
                MapApp
              </h1>
              <p className="text-xs text-slate-500 truncate">
                Draw polygons, place objects, inspect data
              </p>
            </div>
          </div>

          <div className="flex items-center justify-end gap-3">
            <div className="hidden sm:flex items-center gap-2 rounded-full bg-slate-900/5 px-3 py-1 text-sm text-slate-700 ring-1 ring-inset ring-slate-900/10">
              <span
                className={[
                  'h-2 w-2 rounded-full',
                  mode === 'view'
                    ? 'bg-emerald-500'
                    : mode === 'draw-polygon'
                      ? 'bg-sky-500'
                      : 'bg-violet-500',
                ].join(' ')}
              />
              <span className="font-medium">{statusLabel}</span>
              <span className="text-slate-500">•</span>
              <span className="text-slate-500">{statusHint}</span>
            </div>

            <div className="sm:hidden inline-flex items-center gap-2 rounded-full bg-slate-900/5 px-3 py-1 text-sm text-slate-700 ring-1 ring-inset ring-slate-900/10">
              <span
                className={[
                  'h-2 w-2 rounded-full',
                  mode === 'view'
                    ? 'bg-emerald-500'
                    : mode === 'draw-polygon'
                      ? 'bg-sky-500'
                      : 'bg-violet-500',
                ].join(' ')}
              />
              <span className="font-medium">{statusLabel}</span>
            </div>
          </div>
        </div>
        <div className="h-px bg-gradient-to-r from-transparent via-sky-500/30 to-transparent" />
      </header>

      <main className="flex-1 min-h-0">
        <div className="mx-auto max-w-screen-2xl px-3 py-3 sm:px-4 sm:py-4 lg:px-6 lg:py-6 h-full">
          <div className="grid h-full min-h-0 grid-cols-1 min-[600px]:grid-cols-[minmax(336px,1fr)_minmax(220px,240px)] lg:grid-cols-[minmax(336px,1fr)_320px] xl:grid-cols-[minmax(336px,1fr)_348px] gap-4 lg:gap-6">
            <section className="min-h-0">
              <div className="h-full min-h-[360px] rounded-2xl overflow-hidden bg-white/70 backdrop-blur ring-1 ring-inset ring-slate-900/10 shadow-sm">
                <MapView />
              </div>
            </section>

            <aside className="min-h-0 flex flex-col gap-4">
              <PolygonPanel />
              <ObjectPanel />
              <DataTablePanel />
            </aside>
          </div>
        </div>
      </main>
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
