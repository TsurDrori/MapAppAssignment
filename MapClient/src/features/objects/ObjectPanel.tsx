import { useCallback } from 'react';
import { Panel } from '@/components/ui/Panel';
import { Button } from '@/components/ui/Button';
import { ObjectTypeSelector } from './ObjectTypeSelector';
import { useMapState, useMapDispatch } from '@/context/MapContext';
import { getErrorMessage } from '@/utils/errors';
import {
  useCreateObjectsBatch,
  useDeleteObject,
} from './hooks/useObjects';

export function ObjectPanel() {
  const {
    mode,
    objectTypeToPlace,
    selectedObjectId,
    pendingObjects,
  } = useMapState();
  const dispatch = useMapDispatch();

  const createObjectsBatch = useCreateObjectsBatch({
    onSuccess: () => {
      dispatch({ type: 'CLEAR_PENDING_OBJECTS' });
      dispatch({ type: 'EXIT_PLACE_OBJECT_MODE' });
    },
  });
  const deleteObject = useDeleteObject({
    onSuccess: () => dispatch({ type: 'SELECT_OBJECT', payload: null }),
  });

  const isPlacing = mode === 'place-object';
  const hasPendingObjects = pendingObjects.length > 0;

  const handleAdd = useCallback(() => {
    dispatch({ type: 'ENTER_PLACE_OBJECT_MODE', payload: objectTypeToPlace });
  }, [dispatch, objectTypeToPlace]);

  const handleCancel = useCallback(() => {
    dispatch({ type: 'EXIT_PLACE_OBJECT_MODE' });
    dispatch({ type: 'CLEAR_PENDING_OBJECTS' });
  }, [dispatch]);

  const handleSave = useCallback(() => {
    if (!hasPendingObjects) return;

    createObjectsBatch.mutate({
      objects: pendingObjects.map((obj) => ({
        location: obj.location,
        objectType: obj.objectType,
      })),
    });
  }, [hasPendingObjects, pendingObjects, createObjectsBatch]);

  const handleDeleteSelected = useCallback(() => {
    if (!selectedObjectId) return;
    deleteObject.mutate(selectedObjectId);
  }, [selectedObjectId, deleteObject]);

  // Inline error for create mutation
  const inlineError = createObjectsBatch.isError
    ? getErrorMessage(createObjectsBatch.error, 'Failed to save')
    : null;

  const handleTypeChange = useCallback((newType: string) => {
    dispatch({ type: 'SET_OBJECT_TYPE', payload: newType });
    if (isPlacing) {
      dispatch({ type: 'ENTER_PLACE_OBJECT_MODE', payload: newType });
    }
  }, [dispatch, isPlacing]);

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

        {/* Inline error display */}
        {inlineError && (
          <div className="rounded-xl bg-rose-500/10 p-3 text-sm ring-1 ring-inset ring-rose-600/20">
            <div className="flex items-start gap-2">
              <svg className="h-5 w-5 flex-shrink-0 text-rose-600" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-5a.75.75 0 01.75.75v4.5a.75.75 0 01-1.5 0v-4.5A.75.75 0 0110 5zm0 10a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
              </svg>
              <p className="flex-1 text-rose-700">{inlineError}</p>
              <button
                onClick={() => createObjectsBatch.reset()}
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
