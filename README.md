# Library Management System

A full-stack Library Management System built for a Software Engineer (.NET) technical
assessment: a Clean Architecture ASP.NET Core API backed by PostgreSQL, and a React SPA
frontend.

- **Backend:** ASP.NET Core 10 Minimal APIs, EF Core, PostgreSQL, MediatR (CQRS), FluentValidation,
  Serilog, JWT auth with role-based authorization.
- **Frontend:** React 19 + Vite + TypeScript, React Router, TanStack Query, Axios, Mantine.

Modules: Authentication & Authorization, Branch Management, Book Management (with per-branch
copy inventory), Member Management, Borrow & Return, Reservation Queue, and Reports — each with
a Dashboard summary and role-based navigation (Admin / Librarian / Member) on the frontend.

## Requirements Coverage

Mapped directly against the assessment brief's own section headers, so each line here is a
literal requirement, not a paraphrase.

<details>
<summary><strong>Functional Requirements</strong></summary>

- [x] Authentication (JWT) & Role-based Authorization — access + refresh tokens, `Admin` /
      `Librarian` / `Member` roles enforced via `.RequireAuthorization()` policies
- [x] Branch Management — CRUD, soft delete, search by name/address
- [x] Book Management — CRUD, soft delete, search by title/author/ISBN/genre, per-branch copy
      inventory
- [x] Member Management — CRUD, search, suspend/reactivate, self-registration auto-creates a
      Member profile
- [x] Borrow & Return Management — business rules enforced (max 5 active loans, no duplicate
      active loan for the same book, no borrowing with zero available copies)
- [x] Reservation Queue — FIFO queue, staff-assisted and self-service creation, automatic
      Pending→Ready promotion on return, ownership-or-staff cancellation
- [x] Reports — overdue loans, most-borrowed books, branch inventory, member activity,
      reservation queue summary
- [x] Standard CRUD operations, search/filtering where appropriate, and basic business
      validations (FluentValidation on every command that takes free-form input)

</details>

<details>
<summary><strong>Frontend Requirements</strong></summary>

- [x] Login / Logout
- [x] Dashboard — live stat cards (role-specific: staff see catalog/branch/overdue/reservation
      counts, Members see their own active loans/reservations)
- [x] Role-based Navigation — nav items and routes both gated by role, not just hidden in the UI
- [x] Branch, Book & Member Management
- [x] Borrow & Return
- [x] Reservation Queue
- [x] Reports
- [x] Responsive UI — audited and fixed at mobile (375px) and tablet (768px) breakpoints

</details>

<details>
<summary><strong>Technical Expectations</strong></summary>

- [x] Clean Architecture (Domain / Application / Infrastructure / Api, dependency rule enforced
      by project references) — see `LibraryManagementApi/ARCHITECTURE.md`
- [x] Dependency Injection throughout (constructor injection, `AddApplicationServices()` /
      `AddInfrastructureServices()` composition roots)
- [x] SOLID Principles — see [Design Decisions & Assumptions](#design-decisions--assumptions)
      for concrete examples per letter
- [x] Design patterns — CQRS/MediatR, Factory (`Entity.Create()` static factories), Specification
      (`OverdueLoanSpecification`); Repository and Strategy deliberately *not* used, with
      reasoning documented below rather than added for their own sake
- [x] FluentValidation — one validator per command that takes free-form input
- [x] Centralized Exception Handling — single `GlobalExceptionHandler`, RFC 9457 `ProblemDetails`
- [x] Logging — Serilog, two-stage bootstrap, request logging
- [x] Secure Coding Practices — JWT + refresh rotation with reuse detection, ASP.NET Identity
      password hashing, CORS locked to the frontend origin, per-IP rate limiting on auth
      endpoints, security response headers, secrets via user-secrets/environment only
- [x] Asynchronous Programming — `async`/`await` end to end, no sync-over-async
- [x] Efficient Database Access — `AsNoTracking()` on every read-only query, paginated list
      endpoints, projection-only DTO queries
- [x] Unit Testing — 313 backend tests (35 Domain, 232 Application, 46 integration against real
      Postgres via Testcontainers)

</details>

<details>
<summary><strong>Deliverables</strong></summary>

- [x] Source Code
- [x] Git Repository
- [x] `README.md` (this file)
- [x] Database Migrations — 7 EF Core migrations, applied via `dotnet ef database update`
- [x] Swagger/OpenAPI — interactive Scalar UI at `/scalar/v1`
- [x] Unit Tests
- [x] Setup Instructions — see [Quick Start](#quick-start)

</details>

<details>
<summary><strong>Bonus (honest status, not all attempted)</strong></summary>

- [x] CQRS
- [x] Email Notifications — password-reset email via MailKit/Gmail SMTP
- [ ] Domain Events
- [ ] Optimistic Concurrency
- [ ] API Versioning
- [ ] Health Checks
- [ ] Docker for the API itself (PostgreSQL is dockerized; the API is not)
- [ ] Redis
- [ ] Background Jobs
- [ ] Excel/PDF Export
- [ ] CI/CD Pipeline

</details>

## Project Structure

```
library-management-system/
  docker-compose.yml              # PostgreSQL for local development
  LibraryManagementApi/           # Backend — see ARCHITECTURE.md for the full layer breakdown
    LibraryManagementApi.slnx
    src/
      LibraryManagementApi.Domain/          # Entities, enums, domain exceptions — zero dependencies
      LibraryManagementApi.Application/     # CQRS commands/queries, validators, interfaces
      LibraryManagementApi.Infrastructure/  # EF Core, Identity, JWT, email — implements Application's interfaces
      LibraryManagementApi.Api/             # Minimal API endpoints, composition root
    tests/
      LibraryManagementApi.Domain.UnitTests/
      LibraryManagementApi.Application.UnitTests/
      LibraryManagementApi.Api.IntegrationTests/   # WebApplicationFactory + Testcontainers (real Postgres)
  library-management-web/         # Frontend
    src/
      app/                        # Router, AppShell layout, navigation config, query client
      features/                   # One folder per module (auth, branches, books, members, loans, reservations, reports, dashboard)
      lib/                        # Shared axios instance, JWT interceptor, error helpers, types
```

## Quick Start

### 1. Start PostgreSQL

From the repo root:

```bash
docker compose up -d
```

This starts `postgres:17-alpine` on `localhost:5432` with database `library_management`,
user `library_admin` / password `library_admin_dev_password` (local dev only — see
[Environment & Secrets](#environment--secrets)).

<details>
<summary><strong>2a. Backend setup (LibraryManagementApi)</strong></summary>

```bash
cd LibraryManagementApi

# One-time: configure secrets (never committed — see below)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=library_management;Username=library_admin;Password=library_admin_dev_password" \
  --project src/LibraryManagementApi.Api

dotnet user-secrets set "Jwt:Secret" "<a long random string, 32+ chars>" \
  --project src/LibraryManagementApi.Api

# Optional — only needed for the forgot-password email flow to actually send:
dotnet user-secrets set "Email:SenderEmail" "you@gmail.com" --project src/LibraryManagementApi.Api
dotnet user-secrets set "Email:Username" "you@gmail.com" --project src/LibraryManagementApi.Api
dotnet user-secrets set "Email:Password" "<gmail app password>" --project src/LibraryManagementApi.Api

# Apply migrations
dotnet ef database update \
  --project src/LibraryManagementApi.Infrastructure \
  --startup-project src/LibraryManagementApi.Api

# Run
dotnet run --project src/LibraryManagementApi.Api
```

The API listens on `http://localhost:5059` (see `launchSettings.json`). On startup it seeds
the three roles (`Admin`, `Librarian`, `Member`) and, **in the Development environment only**,
seeds three ready-to-use accounts — see [Seeded Accounts](#seeded-dev-accounts-development-only).

Interactive API docs (Scalar UI) are available at `http://localhost:5059/scalar/v1` while
running in Development.

**Running tests:**

```bash
dotnet test                                                          # everything
dotnet test tests/LibraryManagementApi.Domain.UnitTests               # pure domain logic
dotnet test tests/LibraryManagementApi.Application.UnitTests          # handler/validator logic (EF InMemory)
dotnet test tests/LibraryManagementApi.Api.IntegrationTests           # full HTTP + real Postgres via Testcontainers — requires Docker running
```

</details>

<details>
<summary><strong>2b. Frontend setup (library-management-web)</strong></summary>

```bash
cd library-management-web
pnpm install

cp .env.example .env   # defaults to http://localhost:5059, matching the backend above

pnpm dev
```

The app runs on `http://localhost:5173`. The backend's CORS policy is locked to this exact
origin (`ClientApp:BaseUrl` in `appsettings.json`) — if you change the frontend's port, update
that setting too.

**Other commands:**

```bash
pnpm build     # type-check + production build
pnpm lint      # eslint
pnpm preview   # preview the production build
```

</details>

## Seeded Dev Accounts (Development only)

Seeded automatically on startup when `ASPNETCORE_ENVIRONMENT=Development` (the default for
`dotnet run`) — never in Production. Configured in `appsettings.Development.json` under `Seed:*`.

| Role | Email | Password |
|---|---|---|
| Admin | `admin@library.local` | `Admin123!` |
| Librarian | `librarian@library.local` | `Librarian123!` |
| Member | `member@library.local` | `Member123!` |

A "Main Branch" is also seeded so the Member account has a home branch to belong to. Without
this seed there would be no way to log in at all on a fresh clone — self-registration always
creates a `Member`, and every other role/mutation requires an existing `Admin`.

## Environment & Secrets

| Where | What | Notes |
|---|---|---|
| `dotnet user-secrets` (backend) | `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, `Email:*` | Never committed. `Jwt:Secret` must be 32+ characters (HMAC-SHA256 key). |
| `appsettings.json` (backend) | `Jwt:Issuer`/`Audience`/expiry, `Email:SmtpHost`/`Port`, `ClientApp:BaseUrl` | Non-secret, committed. |
| `appsettings.Development.json` (backend) | `Seed:AdminEmail`/`AdminPassword`/etc. | Dev-only seed credentials, committed intentionally — see rationale above. |
| `.env` (frontend) | `VITE_API_URL` | Copy from `.env.example`; not committed. |

`appsettings.Production.json` and `appsettings.*.Local.json` are gitignored — production
configuration is expected to come from environment variables or a secrets manager, never a
committed file.

## Design Decisions & Assumptions

- **SOLID — one concrete example per letter, from this codebase:**
  - **S**RP: a command handler does exactly one use case (e.g. `BorrowBookCommandHandler` only
    handles borrowing) rather than a `BookService` god-class with a dozen methods.
  - **O**CP: FluentValidation validators and MediatR pipeline behaviors are added by *registering
    a new class*, not by editing existing handler code — `AddValidatorsFromAssembly` /
    `RegisterServicesFromAssembly` discover new handlers/validators automatically.
  - **L**SP: any `IRequestHandler<TCommand, TResponse>` must be substitutable by MediatR's
    dispatcher with no special-casing — enforced structurally by the interface, not by convention.
  - **I**SP: `IApplicationDbContext` exposes only the `DbSet<T>`s Application actually needs, not
    the full `DbContext` surface.
  - **D**IP: Application depends on interfaces it defines itself (`IApplicationDbContext`,
    `IIdentityService`); Infrastructure depends on Application to implement them — dependencies
    point at the abstraction, never the concrete detail.
- **Clean Architecture (Jason Taylor template)** over Onion/Vertical Slice: the assessment
  explicitly calls out MediatR/CQRS, which maps directly onto Clean's Application layer (one
  Command/Query + Handler per use case), and the four-project split makes the dependency rule
  compiler-enforced rather than convention-only. See `LibraryManagementApi/ARCHITECTURE.md` for
  the full layout and dependency rule.
- **CQRS via MediatR** — every use case is an independent Command/Query + Handler +
  (for commands) FluentValidation validator. A pipeline (`UnhandledExceptionBehaviour` →
  `ValidationBehaviour`) handles cross-cutting concerns so handlers stay focused on business logic.
- **Per-branch copy inventory for Books** (`BookInventory`, keyed by book+branch) rather than a
  single global copy count or individually-barcoded copies — matches how a multi-branch library
  actually operates (a book's availability differs by branch) without the overhead of tracking
  individual physical copies, which nothing in the requirements calls for.
- **Member profiles auto-created on self-registration** — registering *is* what makes someone a
  library member, so the `Member` domain record is created in the same transaction as the
  Identity user rather than requiring a separate staff step. Staff can also register walk-in
  members with no online account (`UserId` is nullable on `Member`).
- **JWT access + refresh tokens, both returned in the response body** (not cookies) — refresh
  token rotation includes reuse detection: presenting an already-rotated token revokes every
  active token for that user, treating reuse as a signal of theft/replay.
- **PATCH semantics**: a `null` field means "leave unchanged," not "clear this field." This is a
  deliberate, simpler subset of the full JSON Patch spec — sufficient for this domain's update
  forms, at the cost of having no way to explicitly null out an optional field via PATCH (a
  known, accepted limitation of this approach).
- **Soft delete throughout** (`Branch.IsActive`, `Book.IsActive`, `Member.Status`,
  `Reservation.Status`) — nothing is hard-deleted once other rows can reference it. Deactivating
  a Branch or Book is a one-way operation with no reactivate endpoint (by design — matches how
  discontinuing a physical branch/catalog entry works in practice); Members can be
  suspended/reactivated since that's a genuinely reversible state.
- **Result pattern for business-rule failures, exceptions for everything else** — a Command
  returns `Result`/`Result<T>` when failure is an expected business outcome (e.g. "no copies
  available"), while `NotFoundException` / `DomainException` / `ForbiddenAccessException` are
  used for the entity-not-found, invariant-violation, and authorization cases respectively, each
  mapped to the correct HTTP status by a single global exception handler (RFC 9457
  `ProblemDetails`) — no `try/catch` in any endpoint.
- **Specification pattern** for the one place a query predicate was genuinely duplicated across
  handlers ("is this loan overdue" — used by both the loans list filter and the overdue-loans
  report): `OverdueLoanSpecification` in `Application/Common/Specifications` encapsulates it once
  as an `Expression<Func<Loan, bool>>`, translatable by EF Core, instead of two independent
  inline `.Where(...)` clauses that could drift out of sync.
- **No generic `IRepository<T>` over EF Core.** `DbContext` + `DbSet<T>` already *are* the
  Repository and Unit of Work patterns; wrapping them again in a generic repository interface is
  a well-known anti-pattern that hides EF Core's own change-tracking instead of composing with
  it, typically without any real abstraction gained. `IApplicationDbContext` (an interface over
  `DbContext` exposing only the `DbSet<T>`s Application needs) is the actual seam used for
  testability — Application-layer tests substitute an EF Core InMemory-backed implementation of
  it, with no repository wrapper needed to make that possible.
- **No Strategy pattern.** Candidate spots exist in theory (e.g. fine calculation per membership
  tier), but nothing in this domain has more than one genuinely varying algorithm today — adding
  Strategy without a second real implementation would be pattern-for-its-own-sake rather than
  solving an actual coupling problem.
- **Dev-only seed accounts** (see above) exist because there is otherwise no bootstrap path to
  the first Admin account on a fresh clone: self-registration always creates a `Member`, and
  Branch/Member/Book management all require an existing `Admin`.
- **Integration tests use Testcontainers for real PostgreSQL, not EF Core's InMemory provider** —
  InMemory doesn't enforce real SQL semantics (unique indexes, FK constraints, provider-specific
  LINQ translation), which matters here: several report queries rely on correlated `COUNT`
  subqueries and `GROUP BY` with conditional aggregates that behave differently across providers.
  This caught two real bugs during development (see below) that Application-layer unit tests
  (which mock the DB) structurally could not have caught.
- **Two bugs the integration test suite caught that unit tests couldn't**: (1) `AddIdentityCore`
  does not register default token providers the way `AddIdentity` does, so
  `GenerateUserTokenAsync` (the forgot-password flow) threw on every call until
  `.AddDefaultTokenProviders()` was added; (2) enums were serializing as raw integers instead of
  strings in every DTO, invisible to C#-to-C# test round-tripping (both sides agreed on the same
  numeric encoding) but broken for a real JSON consumer — fixed with a global
  `JsonStringEnumConverter`.

## API Documentation

With the backend running in Development, open **`http://localhost:5059/scalar/v1`** for an
interactive API reference (built on the OpenAPI document generated by
`Microsoft.AspNetCore.OpenApi`) — browse every endpoint, see request/response schemas, and send
requests directly from the browser with a bearer token.
