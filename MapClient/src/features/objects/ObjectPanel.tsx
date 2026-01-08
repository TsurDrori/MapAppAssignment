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
    <Panel title="Objects">
      <div className="space-y-3">
        {/* Object type selector */}
        <ObjectTypeSelector
          value={objectTypeToPlace}
          onChange={handleTypeChange}
          disabled={mode === 'draw-polygon'}
        />

        {/* Mode indicator */}
        {isPlacing && (
          <div className="text-sm text-blue-600 bg-blue-50 p-2 rounded">
            Click on map to place {objectTypeToPlace} markers.
            {hasPendingObjects && (
              <span className="block mt-1">
                {pendingObjects.length} object(s) pending. Click Save to confirm.
              </span>
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
          <div className="text-xs text-gray-500">
            Selected: {selectedObjectId.slice(-8)}
          </div>
        )}

        {/* Keyboard hint */}
        {isPlacing && (
          <div className="text-xs text-gray-400">Press Escape to cancel</div>
        )}
      </div>
    </Panel>
  );
}
