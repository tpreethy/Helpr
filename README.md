# Helpr

An AI-assisted support ticket system. Ingests emails from Gmail, classifies and summarizes each ticket with AI, drafts a reply from a knowledge base, and lets a human agent review, edit, and send — all in one loop.

## Stack

| Layer | Technology |
|---|---|
| Frontend | React 19 + TypeScript, Vite 6, Bun runtime |
| Backend | .NET 10, ASP.NET Core Web API (controller-based) |
| ORM | EF Core 10 + Npgsql |
| Database | PostgreSQL |
| Auth | ASP.NET Core Identity + JWT Bearer |
| AI | Anthropic Claude (claude-haiku) |
| Email | Google Gmail API |

## Project Structure

```
Helpr.slnx
├── src/
│   ├── Helpr.Core/           # Entities, interfaces, enums — no external deps
│   ├── Helpr.Infrastructure/ # EF Core, repositories, migrations, external services
│   └── Helpr.API/            # Controllers, DI wiring, background services
└── client/                   # React frontend
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (v22+) and [Bun](https://bun.sh)
- PostgreSQL running locally on port 5432

### Backend

1. Create a `helpr` database in your local PostgreSQL instance.

2. Update connection string in `src/Helpr.API/appsettings.json` if your credentials differ from the defaults (`postgres/postgres`).

3. Apply migrations and run:

```bash
dotnet ef database update --project src/Helpr.Infrastructure --startup-project src/Helpr.API
dotnet run --project src/Helpr.API --launch-profile http
```

API runs at `http://localhost:5262`. Swagger UI at `http://localhost:5262/swagger`.

### Frontend

```bash
cd client
npm install   # use npm, not bun install (see note in CLAUDE.md)
bun dev
```

Frontend runs at `http://localhost:5173`.

## Configuration

Key settings in `src/Helpr.API/appsettings.json`:

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:Key` | JWT signing secret (change before deploying) |
| `Admin:Email` / `Admin:Password` | Seeded admin account |

For Gmail and Anthropic API keys, add to `appsettings.Development.json` (not committed):

```json
{
  "Gmail": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "Anthropic": {
    "ApiKey": "..."
  }
}
```
