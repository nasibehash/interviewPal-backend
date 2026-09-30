import { useReducer } from 'react';

// در React «فرمان» معمولاً یک action است و undo با نگه داشتن تاریخچهٔ state ساخته می‌شود
interface History<T> {
  past: T[];
  present: T;
  future: T[];
}

type Action<T> = { type: 'set'; value: T } | { type: 'undo' } | { type: 'redo' };

function reducer<T>(state: History<T>, action: Action<T>): History<T> {
  switch (action.type) {
    case 'set':
      return { past: [...state.past, state.present], present: action.value, future: [] };
    case 'undo': {
      const previous = state.past.at(-1);
      if (previous === undefined) return state;
      return { past: state.past.slice(0, -1), present: previous, future: [state.present, ...state.future] };
    }
    case 'redo': {
      const [next, ...rest] = state.future;
      if (next === undefined) return state;
      return { past: [...state.past, state.present], present: next, future: rest };
    }
  }
}

export function useUndoRedo<T>(initial: T) {
  const [state, dispatch] = useReducer(reducer<T>, { past: [], present: initial, future: [] });
  return {
    value: state.present,
    set: (value: T) => dispatch({ type: 'set', value }),
    undo: () => dispatch({ type: 'undo' }),
    redo: () => dispatch({ type: 'redo' }),
    canUndo: state.past.length > 0,
    canRedo: state.future.length > 0,
  };
}

export function NoteEditor() {
  const note = useUndoRedo('');
  return (
    <>
      <textarea value={note.value} onChange={(e) => note.set(e.target.value)} aria-label="یادداشت" />
      <button disabled={!note.canUndo} onClick={note.undo}>بازگردانی</button>
      <button disabled={!note.canRedo} onClick={note.redo}>از نو</button>
    </>
  );
}
