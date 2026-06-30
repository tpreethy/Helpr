# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Helpr is an AI-assisted support ticket system. The MVP core loop: Gmail ingests emails → AI classifies, summarizes, and drafts a reply from a hardcoded knowledge base → a human agent reviews, edits, and sends. All other features (KB management UI, dashboard, audit trail, user management UI) are deferred post-MVP.

## Commands

### Backend (.NET 10)

```powershell
# Build the full solution
dotnet build Helpr.slnx

# Run the API (http://localhost:5262, Swagger at /swagger)
dotnet run --project src/Helpr.API --launch-profile http

# EF Core migrations (always specify both --project and --startup-project)
dotnet ef migrations add <Name> --project src/Helpr.Infrastructure --startup-project src/Helpr.API
dotnet ef database update   --project src/Helpr.Infrastructure --startup-project src/Helpr.API
dotnet ef migrations remove --project src/Helpr.Infrastructure --startup-project src/Helpr.API
```

### Frontend (React + Vite)

```powershell
# Install dependencies — always use npm, not bun install (see note below)
cd client && npm install

# Dev server (http://localhost:5173 or next available port)
cd client && bun dev

# Type-check
cd client && npx tsc --noEmit

# Lint
cd client && npm run lint
```

> **Bun/npm note:** This machine runs Bun as ARM64 but Node.js as x64. `bun install` downloads ARM64 native bindings, which breaks Rollup/Vite. Always use `npm install` to install packages; `bun dev` is fine for running scripts.

## Architecture

### Solution layout

```
Helpr.slnx
├── src/
│   ├── Helpr.Core/           # Pure C# — entities, interfaces, enums. Zero NuGet deps.
│   ├── Helpr.Infrastructure/ # EF Core, Npgsql, repository implementations, migrations
│   └── Helpr.API/            # ASP.NET Core controllers, DI wiring, background services
└── client/                   # React 19 + TypeScript + Vite 6
```

**Dependency direction:** `API → Infrastructure → Core`. Core knows nothing about the other two projects.

### Adding new backend features

- **Entities and interfaces** belong in `Helpr.Core`. No external package references allowed here.
- **Repository implementations and EF config** go in `Helpr.Infrastructure/Persistence/`.
- **Controllers and DTOs** go in `Helpr.API/Controllers/`. Return DTOs, never EF entities directly.
- Infrastructure is wired into DI via the `AddInfrastructure()` extension in `Helpr.Infrastructure/DependencyInjection.cs`. Register new services there.
- Background services (e.g., Gmail polling) are `IHostedService` implementations registered in `Program.cs`.

### Configuration keys (`appsettings.json`)

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | Npgsql connection string |
| `Jwt:Key` / `Jwt:Issuer` / `Jwt:Audience` | JWT auth (Phase 3) |
| `Admin:Email` / `Admin:Password` | Seeded admin account credentials |

### EF Core

- `HelprDbContext` lives in `Helpr.Infrastructure/Persistence/`.
- Entity configurations use Fluent API — keep them in separate `IEntityTypeConfiguration<T>` classes, not inline in `OnModelCreating`.
- Migrations live in `Helpr.Infrastructure/Migrations/`. The startup project for `dotnet ef` is always `Helpr.API`.

### Frontend

- Entry: `client/src/main.tsx` → `App.tsx`.
- No routing library installed yet; add React Router when the first multi-page flow is needed.
- API base URL should be configured via Vite's `VITE_API_URL` env variable, not hardcoded.
