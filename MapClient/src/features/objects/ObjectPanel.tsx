import { Panel } from '@/components/ui/Panel';
import { Button } from '@/components/ui/Button';
import { ObjectTypeSelector } from './ObjectTypeSelector';
import { useMapState, useMapDispatch } from '@/context/MapContext';
import {
  useCreateObjectsBatch,
  useDeleteObject,
  useDeleteAllObjects,
} from './hooks/useObjects';

export function ObjectPanel() {
  const {
    mode,
    objectTypeToPlace,
    selectedObjectId,
    pendingObjects,
  } = useMapState();
  const dispatch = useMapDispatch();

  const createObjectsBatch = useCreateObjectsBatch();
  const deleteObject = useDeleteObject();
  const deleteAllObjects = useDeleteAllObjects();

  const isPlacing = mode === 'place-object';
  const hasPendingObjects = pendingObjects.length > 0;

  const handleAdd = () => {
    dispatch({ type: 'ENTER_PLACE_OBJECT_MODE', payload: objectTypeToPlace });
  };

  const handleCancel = () => {
    dispatch({ type: 'EXIT_PLACE_OBJECT_MODE' });
    dispatch({ type: 'CLEAR_PENDING_OBJECTS' });
  };

  const handleSave = () => {
    if (!hasPendingObjects) return;

    createObjectsBatch.mutate(
      {
        objects: pendingObjects.map((obj) => ({
          location: obj.location,
          objectType: obj.objectType,
        })),
      },
      {
        onSuccess: () => {
          dispatch({ type: 'CLEAR_PENDING_OBJECTS' });
          dispatch({ type: 'EXIT_PLACE_OBJECT_MODE' });
        },
      }
    );
  };

  const handleDeleteSelected = () => {
    if (!selectedObjectId) return;

    deleteObject.mutate(selectedObjectId, {
      onSuccess: () => {
        dispatch({ type: 'SELECT_OBJECT', payload: null });
      },
    });
  };

  const handleDeleteAll = () => {
    if (confirm('Are you sure you want to delete all objects?')) {
      deleteAllObjects.mutate(undefined, {
        onSuccess: () => {
          dispatch({ type: 'SELECT_OBJECT', payload: null });
        },
      });
    }
  };

  const handleTypeChange = (newType: string) => {
    dispatch({ type: 'SET_OBJECT_TYPE', payload: newType });
    if (isPlacing) {
      dispatch({ type: 'ENTER_PLACE_OBJECT_MODE', payload: newType });
    }
  };

  return (
    <Panel
      title="Objects"
      description="Choose a type and place markers on the map"
    >
      <div className="space-y-3">
        {/* Object type selector */}
        <ObjectTypeSelector
          value={objectTypeToPlace}
          onChange={handleTypeChange}
          disabled={mode === 'draw-polygon'}
        />

        {/* Mode indicator */}
        {isPlacing && (
          <div className="rounded-xl bg-violet-500/10 p-3 text-sm ring-1 ring-inset ring-violet-600/20">
            <div className="text-slate-700">
              <span className="font-semibold text-violet-700">Placing:</span>{' '}
              Click on the map to place {objectTypeToPlace} markers.
            </div>
            {hasPendingObjects && (
              <p className="mt-1 text-slate-600">
                {pendingObjects.length} pending. Click Save to confirm.
              </p>
            )}
          </div>
        )}

        {/* Action buttons */}
        <div className="flex flex-wrap gap-2">
          {!isPlacing ? (
            <Button onClick={handleAdd} disabled={mode !== 'view'}>
              Add
            </Button>
          ) : (
            <>
              <Button
                onClick={handleSave}
                disabled={!hasPendingObjects}
                isLoading={createObjectsBatch.isPending}
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
            disabled={!selectedObjectId || isPlacing}
            isLoading={deleteObject.isPending}
          >
            Delete Selected
          </Button>

          <Button
            variant="danger"
            onClick={handleDeleteAll}
            disabled={isPlacing}
            isLoading={deleteAllObjects.isPending}
          >
            Delete All
          </Button>
        </div>

        {/* Selected object info */}
        {selectedObjectId && !isPlacing && (
          <div className="inline-flex items-center gap-2 rounded-full bg-slate-900/5 px-3 py-1 text-xs text-slate-600 ring-1 ring-inset ring-slate-900/10">
            <span className="h-1.5 w-1.5 rounded-full bg-violet-500" />
            <span>Selected: {selectedObjectId.slice(-8)}</span>
          </div>
        )}

        {/* Keyboard hint */}
        {isPlacing && (
          <div className="text-xs text-slate-500">Press Escape to cancel</div>
        )}
      </div>
    </Panel>
  );
}
