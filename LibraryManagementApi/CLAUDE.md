# LibraryManagementApi

Backend for the Library Management System technical assessment. See the root
`Technical Assesment for Software Engineer (.NET) Role_July 2026.md` for the full requirements
and evaluation rubric.

## Architecture

**Clean Architecture (Jason Taylor template).** The full project layout, dependency rule, CQRS
conventions, and per-feature checklist are documented in [ARCHITECTURE.md](./ARCHITECTURE.md) —
read it before adding any feature. Every module (Auth, Branch, Book, Member, Borrow/Return,
Reservation, Reports) follows the exact same shape: Domain entity → Application
Command/Query+Handler+Validator → Infrastructure persistence → Api minimal-endpoint.

Do not put business logic in the `Api` project or reach into `Infrastructure` from `Application` —
see ARCHITECTURE.md's dependency rule diagram if unsure where something belongs.

## Tech Stack

- ASP.NET Core Minimal APIs, .NET 10
- EF Core + PostgreSQL
- MediatR (CQRS)
- FluentValidation
- Serilog
- JWT bearer auth + role-based authorization
- xUnit for tests, WebApplicationFactory + Testcontainers for integration tests

## Commands

Run from the `LibraryManagementApi/` folder (contains `LibraryManagementApi.slnx`):

```bash
dotnet build
dotnet run --project src/LibraryManagementApi.Api
dotnet test
dotnet ef migrations add <Name> --project src/LibraryManagementApi.Infrastructure --startup-project src/LibraryManagementApi.Api
dotnet ef database update --project src/LibraryManagementApi.Infrastructure --startup-project src/LibraryManagementApi.Api
```

## Conventions

- Endpoints are thin: parse request → `IMediator.Send(command/query)` → `TypedResults`. No
  logic in the endpoint body.
- Never return Domain entities from an endpoint or query handler — always a DTO.
- Validate with FluentValidation in Application, not manually in endpoints.
- Exceptions map to RFC 9457 `ProblemDetails` via the global exception-handling middleware — don't
  try/catch in endpoints just to shape a response.
- Secrets never go in `appsettings.json`/`appsettings.Development.json`. Use user-secrets locally;
  `appsettings.Production.json` and `appsettings.*.Local.json` are gitignored.
- Async all the way down (`Async` suffix, `CancellationToken` propagated from the endpoint).

## Testing

- Application handlers: unit tests with mocked `IApplicationDbContext`/services.
- Endpoints/persistence: integration tests via `WebApplicationFactory<Program>` +
  Testcontainers for PostgreSQL (not an in-memory provider — EF Core's InMemory provider hides
  real SQL/constraint bugs).

## Relevant Skills

Load these `dotnet-claude-kit` skills as needed rather than improvising conventions:
`clean-architecture`, `minimal-api`, `ef-core`, `error-handling`, `authentication`,
`dependency-injection`, `testing`, `serilog`, `api-versioning` (bonus), `caching` (bonus).
