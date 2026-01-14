# MapClient Architecture & Design Decisions

A comprehensive guide to the architectural choices in this React frontend, demonstrating patterns appropriate for a senior engineering role.

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [State Management](#2-state-management)
3. [Performance Optimizations](#3-performance-optimizations)
4. [Type Safety](#4-type-safety)
5. [Error Handling](#5-error-handling)
6. [API Layer Design](#6-api-layer-design)
7. [Component Patterns](#7-component-patterns)
8. [Key Design Decisions](#8-key-design-decisions)

---

## 1. Project Structure

### Feature-Based Organization

```
src/
├── api/              → API client layer
│   ├── client.ts     → Generic fetch wrapper with error handling
│   ├── polygons.ts   → Polygon endpoints
│   └── objects.ts    → Object endpoints
├── components/ui/    → Reusable, feature-agnostic components
│   ├── Button.tsx
│   ├── Panel.tsx
│   ├── Table.tsx
│   ├── Toast.tsx
│   └── ErrorBoundary.tsx
├── config/           → Constants and configuration
│   └── markerTypes.ts
├── context/          → Global UI state
│   └── MapContext.tsx
├── features/         → Feature modules (self-contained)
│   ├── map/
│   │   ├── MapView.tsx
│   │   ├── PolygonLayer.tsx
│   │   ├── ObjectLayer.tsx
│   │   ├── DrawingLayer.tsx
│   │   └── MapEventHandler.tsx
│   ├── polygons/
│   │   ├── PolygonPanel.tsx
│   │   └── hooks/usePolygons.ts
│   ├── objects/
│   │   ├── ObjectPanel.tsx
│   │   ├── ObjectTypeSelector.tsx
│   │   └── hooks/useObjects.ts
│   └── data-table/
│       └── DataTablePanel.tsx
├── types/            → TypeScript type definitions
│   └── api.types.ts
├── utils/            → Pure utility functions
│   ├── validation.ts
│   └── errors.ts
└── App.tsx           → Root component with layout
```

### Why Feature-Based?

| Approach                                           | Pros                              | Cons                              |
| -------------------------------------------------- | --------------------------------- | --------------------------------- |
| **Type-based** (`components/`, `hooks/`, `utils/`) | Familiar, simple                  | Features scattered across folders |
| **Feature-based**                                  | Co-located code, clear boundaries | Slightly more nesting             |

**Benefits:**

- Adding a feature = adding one folder
- Deleting a feature = deleting one folder
- Related code lives together (component + hook + styles)
- Scales well as application grows

---

## 2. State Management

### Split Context Pattern

The application separates UI state from server state and uses split contexts for performance.

**Why Split Contexts?**

```
Problem: Single context causes all consumers to re-render on any state change

┌─────────────────────────────────────┐
│  MapContext (state + dispatch)      │
└─────────────────────────────────────┘
         │
    ┌────┴────┬────────────┐
    ▼         ▼            ▼
 Button    PolygonLayer  ObjectPanel
 (needs    (needs        (needs
 dispatch)  state)        both)

Button re-renders when state changes ❌ (it only needs dispatch!)
```

```
Solution: Split into two contexts

┌──────────────┐  ┌───────────────┐
│ StateContext │  │DispatchContext│
└──────────────┘  └───────────────┘
       │                  │
       ▼                  ▼
  PolygonLayer        Button
  (subscribes to    (subscribes to
   state only)       dispatch only)

Button doesn't re-render when state changes ✅
```

### Server State with React Query

**Why React Query instead of Redux/Zustand?**

| Approach        | Use Case                                    | Complexity |
| --------------- | ------------------------------------------- | ---------- |
| Redux           | Complex client state, time-travel debugging | High       |
| Zustand         | Simple global client state                  | Low        |
| **React Query** | Server state (fetching, caching, sync)      | Medium     |
| React Context   | UI state (modals, selections, modes)        | Low        |

Our app has:

- **Server state**: Polygons and objects from API → React Query
- **UI state**: Drawing mode, selections, pending items → Context + useReducer

React Query provides:

- Automatic caching and invalidation
- Loading/error states
- Background refetching
- Optimistic updates (if needed)
- Deduplication of requests

### Discriminated Union Actions

**Benefits:**

- TypeScript enforces valid action shapes
- Exhaustive switch checking in reducer
- IDE autocomplete for action types
- Compile-time errors for invalid dispatches

---

## 3. Performance Optimizations

### Icon Caching with useMemo

**Why?**

- Leaflet icons are expensive to create (DOM operations)
- Without caching: 100 markers = 100 icon instances
- With caching: 100 markers = 3-4 icon instances (one per type)

### Component Memoization

**When to use `memo`:**

- Components that receive stable props but have expensive parents
- Components rendered in lists
- Components that render frequently but rarely change

**When NOT to use `memo`:**

- Components with frequently changing props
- Simple components (overhead > benefit)

### Lazy Loading

**Why lazy load DataTablePanel?**

- Not visible on initial render (collapsed by default)
- Contains Table component with potentially complex rendering
- Reduces initial JavaScript bundle size
- User sees map faster

---

## 4. Type Safety

### Strict TypeScript Configuration

**What strict mode enables:**

- `noImplicitAny`: All variables must have explicit types
- `strictNullChecks`: null/undefined handled explicitly
- `strictFunctionTypes`: Function parameter types checked correctly
- `noImplicitReturns`: All code paths must retur

---

## 5. Error Handling

### Three-Layer Strategy

```
┌─────────────────────────────────────────────┐
│  Layer 1: Error Boundary (React errors)     │
│  - Catches render errors                    │
│  - Shows fallback UI                        │
│  - Prevents full app crash                  │
└─────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────┐
│  Layer 2: API Error Handling                │
│  - ApiError class with status/detail        │
│  - Network error detection                  │
│  - HTTP status mapping                      │
└─────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────┐
│  Layer 3: Toast Notifications               │
│  - User-friendly error messages             │
│  - Success confirmations                    │
│  - Auto-dismiss after 4 seconds             │
└─────────────────────────────────────────────┘
```

## 6. Key Design Decisions

### Decision 1: Context vs Redux

**Choice:** React Context + useReducer for UI state, React Query for server state

**Rationale:**

- App has simple UI state (mode, selections)
- Server state is primary complexity
- React Query handles server state better than Redux
- No need for Redux DevTools or middleware

### Decision 2: Feature-Based vs Type-Based Structure

**Choice:** Feature-based organization

**Rationale:**

- Features are self-contained units
- Easier to navigate (all polygon code in one place)
- Scales better as app grows
- Simpler to delete/refactor features

### Decision 3: Validation Location

**Choice:** Client-side validation before API calls

**Rationale:**

- Immediate user feedback
- Reduces unnecessary API calls
- Server still validates (defense in depth)
- Validation rules are simple

### Decision 4: Toast vs Inline Errors

**Choice:** Toast notifications for mutations, inline for form validation

**Rationale:**

- Toasts: Non-blocking, consistent, auto-dismiss
- Inline: Contextual, persistent until fixed
- Mutations (create/delete) → Toast
- Form input (coordinates) → Inline

### Decision 5: No Global State Library

**Choice:** React Context instead of Redux/Zustand/Jotai

**Rationale:**

- UI state is simple (mode, selections, pending items)
- React Query handles the complex part (server state)
- Less boilerplate, fewer dependencies
- Split context pattern handles performance

---

## Summary

| Category         | Choice               | Why                                     |
| ---------------- | -------------------- | --------------------------------------- |
| **Structure**    | Feature-based        | Co-location, scalability                |
| **UI State**     | Context + useReducer | Simple, built-in, split for performance |
| **Server State** | React Query          | Caching, loading states, mutations      |
| **Types**        | Strict TypeScript    | Catch errors at compile time            |
| **Errors**       | Boundary + Toast     | Graceful degradation + user feedback    |
| **Performance**  | memo, useMemo, lazy  | Prevent unnecessary work                |
| **API**          | Typed client + hooks | Type safety, encapsulation              |
