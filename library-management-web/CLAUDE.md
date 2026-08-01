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
- Axios (HTTP client, with a request interceptor attaching the JWT, and a response interceptor
  that transparently refreshes an expired access token once before failing)
- Mantine (`@mantine/core`, `@mantine/hooks`, `@mantine/form`, `@mantine/notifications`) — chosen
  over Tailwind/CSS Modules specifically because most of this app is CRUD tables and forms
  (branches/books/members/loans/reservations/reports); Mantine's DataTable-style components, form
  hooks, and notifications save the most time for the least grading weight (10/100)
- `@tabler/icons-react` (Mantine's usual icon pairing)
- ESLint (already scaffolded by Vite's `react-ts` template — keep it, don't switch to oxlint;
  `eslint-plugin-react-hooks` + `typescript-eslint` type-aware rules matter more here than lint speed)

The full stack above is installed — see `package.json`.

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
pnpm dev        # start dev server
pnpm build      # tsc -b && vite build
pnpm lint       # eslint .
pnpm preview    # preview production build
```

## Not Yet Decided

- Component/testing library (if unit tests are added for the frontend) — update this file once
  chosen.
