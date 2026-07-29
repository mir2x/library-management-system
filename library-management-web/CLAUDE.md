# library-management-web

Frontend for the Library Management System technical assessment — a client-side SPA consuming
the `LibraryManagementApi` REST API (JWT-secured). See the root
`Technical Assesment for Software Engineer (.NET) Role_July 2026.md` for full requirements;
frontend is a minor share of the grading (10/100) vs. the API's architecture/backend criteria —
don't over-engineer this side.

## Tech Stack

- Vite + React 19 + TypeScript
- React Router (client-side routing, role-based route guards)
- TanStack Query (server state / data fetching, caching, invalidation)
- Axios (HTTP client, with a request interceptor attaching the JWT)
- ESLint (already scaffolded by Vite's `react-ts` template — keep it, don't switch to oxlint;
  `eslint-plugin-react-hooks` + `typescript-eslint` type-aware rules matter more here than lint speed)

Note: `react-router-dom`, `@tanstack/react-query`, and `axios` are the agreed stack but may not be
installed yet — check `package.json` before assuming they're present.

## Folder Structure (target)

Organize by feature, not by file type:

```
src/
  app/
    routes.tsx              # React Router route tree, role-guarded routes
    queryClient.ts          # TanStack QueryClient instance
  lib/
    api.ts                  # axios instance, baseURL from VITE_API_URL, JWT interceptor
  features/
    auth/                   # login/logout, auth context, token storage
    branches/
    books/
    members/
    borrowing/              # borrow & return
    reservations/
    reports/
    dashboard/
  components/                # shared/dumb UI components only
main.tsx                      # QueryClientProvider + BrowserRouter wrap App
```

Each feature folder owns its own API calls (TanStack Query hooks, e.g. `useBooks.ts`,
`useCreateBook.ts`), types, and components — don't centralize all API calls into one giant file.

## Conventions

- Data fetching goes through TanStack Query hooks (`useQuery`/`useMutation`) calling the shared
  `axios` instance from `lib/api.ts` — no `fetch` calls scattered in components.
- JWT is attached via an axios request interceptor, not manually per-call.
- Role-based navigation/route guards live in `app/routes.tsx` — check the user's role from the
  decoded JWT/auth context before rendering protected routes or nav items.
- `.env` holds `VITE_API_URL`; never commit real `.env` (gitignored at the repo root already).
  Commit an `.env.example` instead.

## Commands

```bash
npm run dev        # start dev server
npm run build       # tsc -b && vite build
npm run lint         # eslint .
npm run preview      # preview production build
```

## Not Yet Decided

- Styling approach (plain CSS / CSS Modules / Tailwind) — update this file once chosen.
- Component/testing library (if unit tests are added for the frontend) — update this file once
  chosen.
