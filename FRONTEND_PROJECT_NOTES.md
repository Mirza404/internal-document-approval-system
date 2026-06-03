# Frontend Project Notes

Quick reference for explaining the frontend auth, API, and hooks structure.

## Auth Folder

The `frontend/src/auth` folder handles frontend authentication state and Microsoft auth setup.

- `authContext.ts`
  - Defines the shape of auth state with `AuthContextValue`.
  - Creates the React context with `createContext`.
  - Uses `.ts` because it contains only TypeScript, no JSX.

- `AuthContext.tsx`
  - Defines `AuthProvider`, the component that wraps the app.
  - Stores `token`, `user`, and `isAuthenticated`.
  - Provides `setSession` and `clearSession` to the rest of the app.
  - Uses `.tsx` because it renders JSX:
    `<AuthContext.Provider>...</AuthContext.Provider>`.

- `authStorage.ts`
  - Saves and loads the auth token and user from `localStorage`.
  - Checks whether the JWT is expired.
  - Clears invalid or expired sessions.

- `msal.ts`
  - Configures Microsoft authentication through MSAL.
  - Reads Microsoft client/tenant/scope settings from Vite environment variables.
  - Initializes MSAL and gets Microsoft/API access tokens.

Key point: `authContext.ts` defines the context, while `AuthContext.tsx` provides the actual React provider component.

## API Folder

The `frontend/src/api` folder is the frontend's typed wrapper around backend endpoints.

Instead of components calling Axios directly, API files expose functions like `getDocuments()`, `localLogin()`, or `getPendingApprovals()`.

- `axios.ts`
  - Creates the shared Axios instance.
  - Sets the backend base URL from `VITE_API_BASE_URL`, defaulting to `http://localhost:5210`.
  - Adds the `Authorization: Bearer ...` header before requests.
  - Uses the saved app token first, then falls back to a Microsoft API token.

- `client.ts`
  - Small helper around Axios.
  - Exposes `get`, `post`, `put`, and `delete`.
  - Returns `res.data` directly so API files stay simple.

- `auth.ts`
  - Login/register/current-user backend calls.
  - Handles both local auth and Microsoft auth endpoints.

- `documents.ts`
  - Document CRUD calls.
  - Includes "all documents", "my documents", single document, create, update, delete, and approval history.

- `approvals.ts`
  - Approval-related calls.
  - Gets approvals, pending approvals, creates/updates approvals, and sends approval decisions.

- `documentCatalog.ts`
  - Document category and document type calls.
  - Used for managing catalog/reference data.

- `adminUsers.ts`
  - Admin user management calls.
  - Gets users and updates role/status.

- `notifications.ts`
  - Notification calls.
  - Gets notifications and marks one/all as read.

Typical request flow:

```txt
Component
  -> custom hook
  -> API function
  -> apiClient
  -> shared Axios instance
  -> backend API
```

## Hooks Folder

The `frontend/src/hooks` folder connects React components to API functions, mostly through TanStack Query.

The API folder knows how to call the backend. The hooks folder knows how React should fetch, cache, mutate, and refresh that data.

- `useAuth.ts`
  - Wraps `useContext(AuthContext)`.
  - Throws an error if used outside `AuthProvider`.
  - Gives components access to `user`, `token`, `isAuthenticated`, `setSession`, and `clearSession`.

- `useDocuments.ts`
  - Query hooks for documents:
    `useDocuments`, `useMyDocuments`, `useDocument`, `useMyDocument`.
  - Mutation hooks for create/update/delete.
  - Invalidates document queries after mutations so the UI refreshes.

- `useApprovals.ts`
  - Query hooks for approvals and pending approvals.
  - Mutation hooks for creating/updating approvals and approving/rejecting/requesting changes.
  - Invalidates approvals and documents after decisions.

- `useDocumentCatalog.ts`
  - Query hooks for document categories and document types.
  - Mutation hooks for creating/updating/deleting categories and types.
  - Invalidates the relevant catalog query after changes.

- `useAdminUsers.ts`
  - Query hook for admin users.
  - Mutation hooks for updating user role and active status.
  - Invalidates `["admin", "users"]` after admin changes.

- `useNotifications.ts`
  - Fetches notifications every 30 seconds.
  - Mutation hooks mark one notification or all notifications as read.
  - Updates cached notification state and then refetches.

- `index.ts`
  - Re-exports hooks from one place.
  - Lets other files import hooks more cleanly.

Key point: hooks are the React-facing layer. They keep components cleaner by hiding query keys, mutation setup, cache invalidation, and API details.

## How To Explain It

The frontend is split into clear layers:

```txt
auth/
  Manages logged-in user state, token persistence, and Microsoft auth.

api/
  Defines typed functions for backend HTTP endpoints.

hooks/
  Wraps API functions with React Query for caching, loading states, mutations, and refresh behavior.

components/pages/
  Use hooks instead of calling the backend directly.
```

Short version:

> Auth stores who the user is. API files know how to call the backend. Hooks make those backend calls usable from React components with caching and automatic refresh behavior.
