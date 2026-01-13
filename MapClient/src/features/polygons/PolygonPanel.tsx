import { useState } from 'react';
import { Panel } from '@/components/ui/Panel';
import { Button } from '@/components/ui/Button';
import { useMapState, useMapDispatch } from '@/context/MapContext';
import { validatePolygon } from '@/utils/validation';
import { getErrorMessage } from '@/utils/errors';
import {
  useCreatePolygon,
  useDeletePolygon,
} from './hooks/usePolygons';

export function PolygonPanel() {
  const { mode, pendingPolygonPoints, isPolygonClosed, selectedPolygonId } =
    useMapState();
  const dispatch = useMapDispatch();
  const [validationError, setValidationError] = useState<string | null>(null);

  const createPolygon = useCreatePolygon({
    onSuccess: () => dispatch({ type: 'EXIT_DRAW_MODE' }),
  });
  const deletePolygon = useDeletePolygon({
    onSuccess: () => dispatch({ type: 'SELECT_POLYGON', payload: null }),
  });

  const isDrawing = mode === 'draw-polygon';
  const canSave = isPolygonClosed && pendingPolygonPoints.length >= 3;

  const clearErrors = () => {
    setValidationError(null);
    createPolygon.reset(); // Clear mutation error state
  };

  const handleAdd = () => {
    clearErrors();
    dispatch({ type: 'ENTER_DRAW_MODE' });
  };

  const handleCancel = () => {
    clearErrors();
    dispatch({ type: 'EXIT_DRAW_MODE' });
  };

  const handleSave = () => {
    if (!canSave) return;

    // Client-side validation
    const validation = validatePolygon(pendingPolygonPoints);
    if (!validation.valid) {
      setValidationError(validation.error ?? 'Invalid polygon');
      return;
    }

    setValidationError(null);
    createPolygon.mutate({ coordinates: pendingPolygonPoints });
  };

  // Combine validation error with mutation error for inline display
  const inlineError = validationError ||
    (createPolygon.isError ? getErrorMessage(createPolygon.error, 'Failed to save') : null);

  const handleDeleteSelected = () => {
    if (!selectedPolygonId) return;
    deletePolygon.mutate(selectedPolygonId);
  };

  return (
    <Panel
      title="Polygon"
      description="Draw, save, and manage polygon shapes"
    >
      <div className="space-y-3">
        {/* Mode indicator */}
        {isDrawing && (
          <div className="rounded-xl bg-sky-500/10 p-3 text-sm ring-1 ring-inset ring-sky-600/20">
            {isPolygonClosed ? (
              <p className="font-semibold text-emerald-700">
                Polygon closed. Click Save to confirm.
              </p>
            ) : (
              <div className="text-slate-700">
                <span className="font-semibold text-sky-700">Drawing:</span>{' '}
                Click on the map to add points ({pendingPolygonPoints.length}{' '}
                point{pendingPolygonPoints.length === 1 ? '' : 's'}).
                {pendingPolygonPoints.length >= 3 && (
                  <p className="mt-1 text-slate-600">
                    Click the starting point to close the polygon.
                  </p>
                )}
              </div>
            )}
          </div>
        )}

        {/* Inline error display */}
        {inlineError && (
          <div className="rounded-xl bg-rose-500/10 p-3 text-sm ring-1 ring-inset ring-rose-600/20">
            <div className="flex items-start gap-2">
              <svg className="h-5 w-5 flex-shrink-0 text-rose-600" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-5a.75.75 0 01.75.75v4.5a.75.75 0 01-1.5 0v-4.5A.75.75 0 0110 5zm0 10a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
              </svg>
              <p className="flex-1 text-rose-700">{inlineError}</p>
              <button
                onClick={clearErrors}
                className="flex-shrink-0 text-rose-600 hover:text-rose-800 transition-colors"
                aria-label="Dismiss error"
              >
                <svg className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                  <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
                </svg>
              </button>
            </div>
          </div>
        )}

        {/* Action buttons */}
        <div className="flex flex-wrap gap-2">
          {!isDrawing ? (
            <Button onClick={handleAdd} disabled={mode !== 'view'}>
              Add
            </Button>
          ) : (
            <>
              <Button
                onClick={handleSave}
                disabled={!canSave}
                isLoading={createPolygon.isPending}
              >
                Save
              </Button>
              <Button variant="secondary" onClick={handleCancel}>
                Cancel
              </Button>
            </>
          )}

          <Button
            variant="danger"
            onClick={handleDeleteSelected}
            disabled={!selectedPolygonId || isDrawing}
            isLoading={deletePolygon.isPending}
          >
            Delete Selected
          </Button>
        </div>

        {/* Selected polygon info */}
        {selectedPolygonId && !isDrawing && (
          <div className="inline-flex items-center gap-2 rounded-full bg-slate-900/5 px-3 py-1 text-xs text-slate-600 ring-1 ring-inset ring-slate-900/10">
            <span className="h-1.5 w-1.5 rounded-full bg-sky-500" />
            <span>Selected: {selectedPolygonId.slice(-8)}</span>
          </div>
        )}

        {/* Keyboard hint */}
        {isDrawing && (
          <div className="text-xs text-slate-500">Press Escape to cancel</div>
        )}
      </div>
    </Panel>
  );
}
