## Tech Stack

### Frontend
- **React 19** with **TypeScript**
- **Bun** as runtime, package manager, and bundler
- **Vite** for dev server and production build (via bun)

### Backend
- **.NET 10** — ASP.NET Core Web API (controller-based, not minimal API)
- **EF Core 10** with **Npgsql** (PostgreSQL provider)
- **PostgreSQL** — primary database

---

## Solution Structure

```
Helpr.sln
├── src/
│   ├── Helpr.API/              # ASP.NET Core controllers, middleware, DI wiring
│   ├── Helpr.Core/             # Entities, interfaces, domain enums — no external deps
│   └── Helpr.Infrastructure/   # EF Core DbContext, migrations, repository implementations
└── client/                     # React + TypeScript (Bun + Vite)
```

### Project Responsibilities

**Helpr.Core**
- Entity classes (Ticket, Message, User, etc.)
- Repository interfaces (ITicketRepository, IUserRepository, etc.)
- Service interfaces
- Domain enums (TicketStatus, TicketCategory, UserRole)
- No NuGet dependencies — pure C#

**Helpr.Infrastructure**
- `HelprDbContext` (EF Core)
- EF Core entity configurations (Fluent API)
- Repository implementations
- EF Core migrations
- Dependencies: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`

**Helpr.API**
- ASP.NET Core controllers
- Request/response DTOs
- Middleware (auth, error handling)
- Background service for Gmail polling (`IHostedService`)
- DI registration for infrastructure and services
- Dependencies: `Helpr.Core`, `Helpr.Infrastructure`, `Swashbuckle` (Swagger)

**client/**
- React 19 + TypeScript
- Bun as runtime (`bun dev`), Vite 6 for dev server and bundling
- Use `npm install` for package installation (Bun is ARM64; npm is x64 and downloads the correct native Rollup bindings)

### Project References

```
Helpr.API       → Helpr.Core, Helpr.Infrastructure
Helpr.Infrastructure → Helpr.Core
Helpr.Core      → (none)
```

---

## Additional Libraries

| Concern | Library |
|---|---|
| Auth | ASP.NET Core Identity + JWT Bearer |
| Gmail (read + send) | `Google.Apis.Gmail.v1` |
| Gmail polling | `IHostedService` in Helpr.API (60s interval) |
| AI (classify/summarize/draft) | Anthropic .NET SDK (`Anthropic.SDK`) |
| API docs | Swashbuckle (Swagger UI) |
| Password hashing | ASP.NET Core Identity built-in |
