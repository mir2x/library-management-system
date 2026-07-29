# Architecture — Clean Architecture (Jason Taylor Template)

This API follows Jason Taylor's Clean Architecture layout. Every new feature must slot into this
structure the same way — no shortcuts, no business logic leaking into the API project.

## Target Project Structure

The single `LibraryManagementApi` project will be split into the following projects as features
are added (create a project the first time a feature needs it — don't pre-create empty ones):

```
LibraryManagementApi.sln
src/
  LibraryManagementApi.Domain/           # Enterprise-wide logic. No project references.
    Entities/
    Enums/
    ValueObjects/                        # optional, e.g. Isbn, MembershipNumber
    Exceptions/                          # domain-specific exceptions (e.g. BookNotAvailableException)
    Common/                              # BaseEntity, BaseAuditableEntity, IHasDomainEvents
    Events/                              # domain events (e.g. BookReturnedEvent)

  LibraryManagementApi.Application/      # Use cases. References Domain only.
    Common/
      Interfaces/                        # IApplicationDbContext, ICurrentUserService, IDateTime, IIdentityService
      Behaviours/                        # MediatR pipeline behaviours (see below)
      Mappings/                          # Mapster/AutoMapper profiles or manual mapping extensions
      Models/                            # PaginatedList<T>, Result<T>
      Security/                          # [Authorize] attribute for MediatR requests
    Books/
      Commands/
        CreateBook/
          CreateBookCommand.cs
          CreateBookCommandHandler.cs
          CreateBookCommandValidator.cs
      Queries/
        GetBooks/
          GetBooksQuery.cs
          GetBooksQueryHandler.cs
          BookDto.cs
    Members/ Branches/ Borrowing/ Reservations/ Reports/ Auth/   # same shape per module
    DependencyInjection.cs               # AddApplicationServices(this IServiceCollection)

  LibraryManagementApi.Infrastructure/   # Implements Application interfaces. References Application.
    Persistence/
      ApplicationDbContext.cs
      Configurations/                    # IEntityTypeConfiguration<T> per entity
      Interceptors/                      # auditing, domain event dispatch
      Migrations/
    Identity/                            # JWT issuing, ASP.NET Identity setup
    Services/                            # DateTimeProvider, etc.
    DependencyInjection.cs               # AddInfrastructureServices(this IServiceCollection)

  LibraryManagementApi.Api/              # Composition root. References Application + Infrastructure.
    Endpoints/                           # Minimal API endpoint groups, one file per feature
      BookEndpoints.cs
      MemberEndpoints.cs
    Middleware/                          # global exception handler -> ProblemDetails
    Extensions/                          # WebApplicationBuilder / WebApplication extensions
    Program.cs
    appsettings.json

tests/
  LibraryManagementApi.Domain.UnitTests/
  LibraryManagementApi.Application.UnitTests/
  LibraryManagementApi.Api.IntegrationTests/   # WebApplicationFactory + Testcontainers (PostgreSQL)
```

## The Dependency Rule

```
Domain  <──  Application  <──  Infrastructure
                  ▲
                  └────────────  Api
```

- **Domain** depends on nothing. No EF Core, no MediatR, no ASP.NET references.
- **Application** depends only on **Domain**. Defines interfaces it needs (`IApplicationDbContext`,
  repository/service interfaces) — it never references Infrastructure.
- **Infrastructure** implements the interfaces defined in Application. This is the only project
  allowed to reference EF Core, Npgsql, external HTTP clients, etc.
- **Api** is the composition root: wires DI (`AddApplicationServices()`, `AddInfrastructureServices()`),
  hosts minimal API endpoints, and maps requests to `IMediator.Send(...)`. It must not contain
  business logic — an endpoint handler is a thin translation from HTTP to a Command/Query.

If a new feature seems to require Api → Infrastructure business logic, or Application →
Infrastructure, that's a sign the interface belongs in Application and the implementation in
Infrastructure — fix the boundary, don't bypass it.

## CQRS via MediatR

Every use case is a Command (writes) or Query (reads) living under
`Application/<Module>/Commands|Queries/<UseCase>/`, with three files:

1. `<UseCase>Command.cs` / `<UseCase>Query.cs` — the request record, implements `IRequest<TResponse>`.
2. `<UseCase>CommandHandler.cs` / `<UseCase>QueryHandler.cs` — implements `IRequestHandler<TRequest, TResponse>`.
3. `<UseCase>CommandValidator.cs` — FluentValidation validator for commands that accept input.

Queries return DTOs/projections (`BookDto`, not the `Book` entity) — never leak Domain entities
across the Application boundary to the Api layer.

## Pipeline Behaviours (MediatR)

Registered in this order in `Application/DependencyInjection.cs`:

1. `UnhandledExceptionBehaviour` — logs and rethrows unexpected exceptions.
2. `AuthorizationBehaviour` — enforces `[Authorize(Roles = "...")]` on the request.
3. `ValidationBehaviour` — runs FluentValidation validators, throws `ValidationException` on failure.
4. `PerformanceBehaviour` — logs requests exceeding a latency threshold (e.g. 500ms).

## Cross-Cutting Conventions

- **Validation**: FluentValidation validators in Application, executed by `ValidationBehaviour`.
  Endpoints never validate manually.
- **Exceptions → HTTP**: a single global exception-handling middleware in `Api/Middleware` maps
  `ValidationException` → 400, `NotFoundException` → 404, `ForbiddenAccessException` → 403,
  unhandled → 500, all as RFC 9457 `ProblemDetails`.
- **Persistence**: `ApplicationDbContext` implements `IApplicationDbContext` (defined in
  Application). Use the Specification pattern for reusable, composable query logic (e.g.
  `AvailableBooksSpecification`) instead of ad-hoc repository methods per query.
- **Authentication**: JWT bearer, issued in Infrastructure/Identity. Roles: e.g. `Admin`,
  `Librarian`, `Member` — enforced via the `AuthorizationBehaviour` and/or
  `.RequireAuthorization()` on endpoint groups.
- **Domain events**: raised on entities (`AddDomainEvent(...)`), dispatched by a `SaveChanges`
  interceptor in Infrastructure after a successful commit (e.g. `BookReturnedEvent` →
  notify next reservation in queue).
- **Logging**: Serilog, structured, enriched with correlation ID. No `Console.WriteLine`.

## New Feature Checklist

Before marking any feature "done", it must have:

- [ ] Command/Query + Handler (+ Validator if it takes input) in `Application/<Module>/`
- [ ] DTO(s) for the response — entities never returned directly
- [ ] EF Core configuration if a new entity was introduced (`IEntityTypeConfiguration<T>`)
- [ ] Minimal API endpoint in `Api/Endpoints/`, mapped to the Command/Query, with
      `.RequireAuthorization(...)` where applicable and OpenAPI metadata (`.WithSummary()`, etc.)
- [ ] Unit tests for the handler (Application.UnitTests) and, where it touches persistence/HTTP,
      an integration test (Api.IntegrationTests)
- [ ] No business logic in the endpoint or in Infrastructure — only in Domain/Application

## When Working On This Codebase

Load the matching `dotnet-claude-kit` skill for the area you're touching rather than improvising:
`clean-architecture`, `minimal-api`, `ef-core`, `error-handling`, `authentication`,
`dependency-injection`, `testing`, `serilog`. This file defines *this project's* shape; those
skills define *how* to implement within it.
