import { Panel } from '@/components/ui/Panel';
import { Button } from '@/components/ui/Button';
import { useMapState, useMapDispatch } from '@/context/MapContext';
import {
  useCreatePolygon,
  useDeletePolygon,
  useDeleteAllPolygons,
} from './hooks/usePolygons';

export function PolygonPanel() {
  const { mode, pendingPolygonPoints, isPolygonClosed, selectedPolygonId } =
    useMapState();
  const dispatch = useMapDispatch();

  const createPolygon = useCreatePolygon();
  const deletePolygon = useDeletePolygon();
  const deleteAllPolygons = useDeleteAllPolygons();

  const isDrawing = mode === 'draw-polygon';
  const canSave = isPolygonClosed && pendingPolygonPoints.length >= 3;

  const handleAdd = () => {
    dispatch({ type: 'ENTER_DRAW_MODE' });
  };

  const handleCancel = () => {
    dispatch({ type: 'EXIT_DRAW_MODE' });
  };

  const handleSave = () => {
    if (!canSave) return;

    createPolygon.mutate(
      { coordinates: pendingPolygonPoints },
      {
        onSuccess: () => {
          dispatch({ type: 'EXIT_DRAW_MODE' });
        },
      }
    );
  };

  const handleDeleteSelected = () => {
    if (!selectedPolygonId) return;

    deletePolygon.mutate(selectedPolygonId, {
      onSuccess: () => {
        dispatch({ type: 'SELECT_POLYGON', payload: null });
      },
    });
  };

  const handleDeleteAll = () => {
    if (confirm('Are you sure you want to delete all polygons?')) {
      deleteAllPolygons.mutate(undefined, {
        onSuccess: () => {
          dispatch({ type: 'SELECT_POLYGON', payload: null });
        },
      });
    }
  };

  return (
    <Panel title="Polygon">
      <div className="space-y-3">
        {/* Mode indicator */}
        {isDrawing && (
          <div className="text-sm text-blue-600 bg-blue-50 p-2 rounded">
            {isPolygonClosed ? (
              <span className="text-green-600">
                Polygon closed! Click Save to confirm.
              </span>
            ) : (
              <>
                Click on map to add points ({pendingPolygonPoints.length} points).
                {pendingPolygonPoints.length >= 3 && (
                  <span className="block mt-1">
                    Click the green starting point to close the polygon.
                  </span>
                )}
              </>
            )}
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

          <Button
            variant="danger"
            onClick={handleDeleteAll}
            disabled={isDrawing}
            isLoading={deleteAllPolygons.isPending}
          >
            Delete All
          </Button>
        </div>

        {/* Selected polygon info */}
        {selectedPolygonId && !isDrawing && (
          <div className="text-xs text-gray-500">
            Selected: {selectedPolygonId.slice(-8)}
          </div>
        )}

        {/* Keyboard hint */}
        {isDrawing && (
          <div className="text-xs text-gray-400">Press Escape to cancel</div>
        )}
      </div>
    </Panel>
  );
}
