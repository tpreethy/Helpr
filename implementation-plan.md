## Implementation Plan

Phases build on each other. Each phase should be fully working before moving to the next.

---

## Phase 1 — Solution Scaffold

Goal: empty solution that builds, connects to Postgres, and serves a health check.

- [ ] Create `Helpr.sln` with three projects: `Helpr.Core`, `Helpr.Infrastructure`, `Helpr.API`
- [ ] Add project references (API → Core + Infrastructure, Infrastructure → Core)
- [ ] Install NuGet packages (EF Core, Npgsql, Swashbuckle, Identity, JWT Bearer)
- [ ] Configure `appsettings.json` (connection string, JWT settings)
- [ ] Register `HelprDbContext` in DI (Helpr.Infrastructure)
- [ ] Scaffold `HelprDbContext` with no entities yet; run initial empty migration
- [ ] Add `GET /health` controller to verify API boots
- [ ] Scaffold `client/` with Bun + Vite + React + TypeScript
- [ ] Verify: API returns 200 on `/health`, client dev server loads in browser

---

## Phase 2 — Data Model

Goal: all entities defined, migrated, and accessible via repositories.

**Helpr.Core**
- [ ] Define `User` entity (Id, Email, PasswordHash, Role enum: Admin/Agent)
- [ ] Define `Ticket` entity (Id, GmailThreadId, Subject, Status enum: Open/Resolved, Category, CreatedAt, UpdatedAt)
- [ ] Define `Message` entity (Id, TicketId, GmailMessageId, Body, FromEmail, Direction enum: Inbound/Outbound, SentAt)
- [ ] Define `ITicketRepository` (GetAll, GetById, GetByThreadId, Create, Update)
- [ ] Define `IMessageRepository` (GetByTicketId, Create)
- [ ] Define `IUserRepository` (GetById, GetByEmail, Create)

**Helpr.Infrastructure**
- [ ] Add EF Core entity configurations (table names, FK constraints, indexes on GmailThreadId + GmailMessageId)
- [ ] Register entities in `HelprDbContext`
- [ ] Implement `TicketRepository`, `MessageRepository`, `UserRepository`
- [ ] Register repositories in DI
- [ ] Create and apply EF Core migration

---

## Phase 3 — Auth

Goal: admin can log in and get a JWT; admin can create agent accounts; all other endpoints are protected.

**Backend**
- [ ] Add ASP.NET Core Identity (PasswordHasher only — no Identity UI or cookie auth)
- [ ] Configure JWT Bearer authentication middleware
- [ ] `POST /api/auth/login` — validate credentials, return JWT
- [ ] Seed admin user on startup (email + password from `appsettings`)
- [ ] `POST /api/users` (Admin only) — create an agent account
- [ ] Apply `[Authorize]` globally; exempt `/api/auth/login` and `/health`

**Frontend**
- [ ] Login page (email + password form)
- [ ] Store JWT in memory (not localStorage) with refresh via httpOnly cookie — or localStorage for MVP simplicity
- [ ] Axios/fetch wrapper that attaches `Authorization: Bearer` header
- [ ] Auth context (current user, login, logout)
- [ ] Protected route wrapper — redirect to `/login` if not authenticated
- [ ] Verify: login works, protected routes redirect unauthenticated users

---

## Phase 4 — Gmail Ingestion

Goal: new emails in the Gmail inbox automatically become tickets in the database.

**Helpr.Infrastructure**
- [ ] Add `Google.Apis.Gmail.v1` NuGet package
- [ ] `GmailService` wrapper — authenticate via OAuth2 service account or stored refresh token
- [ ] `FetchNewMessages()` — list unread messages, fetch full message payload, parse sender/subject/body
- [ ] `SendReply(threadId, body)` — send a reply to an existing thread via Gmail API

**Helpr.API**
- [ ] `GmailPollingService : IHostedService` — runs every 60s, calls `FetchNewMessages()`
- [ ] For each new message:
  - Look up `GmailThreadId` in the DB
  - If thread exists and ticket is Resolved → reopen ticket (status = Open), append Message
  - If thread exists and ticket is Open → append Message only
  - If thread not found → create new Ticket + first Message
- [ ] Mark messages as read in Gmail after processing
- [ ] Register `GmailPollingService` in DI

- [ ] Verify: send an email to the inbox, confirm ticket + message rows appear in DB within 60s

---

## Phase 5 — AI Integration

Goal: every new inbound ticket gets classified, summarized, and has a draft reply attached.

**Helpr.Core**
- [ ] Add `AiDraft` entity (Id, TicketId, Category, Summary, DraftReply, GeneratedAt)
- [ ] Define `IAiService` (ClassifyAndDraft(ticketId, emailBody) → AiDraft)

**Helpr.Infrastructure**
- [ ] Install `Anthropic.SDK` NuGet package
- [ ] Define hardcoded knowledge base as a static string constant (FAQ entries, policies)
- [ ] Implement `AnthropicAiService`:
  - Single prompt that returns JSON: `{ category, summary, draftReply }`
  - Category must be one of the admin-configured values (hardcoded list for MVP)
  - Draft reply uses KB content + email context
- [ ] Add `AiDraft` to `HelprDbContext` + migration
- [ ] Register `IAiService` in DI

**Helpr.API**
- [ ] After creating a new ticket in `GmailPollingService`, call `IAiService.ClassifyAndDraft()`
- [ ] Save resulting `AiDraft` row linked to the ticket

- [ ] Verify: new email creates ticket + AiDraft row with populated category, summary, draft

---

## Phase 6 — Ticket API

Goal: full CRUD API for the agent UI to consume.

- [ ] `GET /api/tickets` — list tickets (filter by status, sort by createdAt desc)
- [ ] `GET /api/tickets/{id}` — ticket detail: ticket + all messages + AiDraft
- [ ] `PATCH /api/tickets/{id}/status` — manually update status (Open/Resolved)
- [ ] `POST /api/tickets/{id}/reply` — body: `{ replyText }` → send via Gmail API, save outbound Message, set ticket to Resolved
- [ ] Response DTOs for all endpoints (no EF entities in API responses)
- [ ] Verify endpoints in Swagger UI

---

## Phase 7 — Frontend: Agent Queue

Goal: agent can see all open tickets, review AI draft, edit, and send a reply.

**Ticket List**
- [ ] `GET /api/tickets` call on load
- [ ] Table: Subject, From, Category, Status, Created date
- [ ] Filter toggle: Open / Resolved / All
- [ ] Click row → navigate to ticket detail

**Ticket Detail**
- [ ] `GET /api/tickets/{id}` call on load
- [ ] Display: subject, sender, AI category badge, AI summary
- [ ] Message thread (inbound messages in order)
- [ ] AI draft reply in an editable textarea (pre-filled)
- [ ] Send button → `POST /api/tickets/{id}/reply` → success redirects back to list
- [ ] Loading and error states

---

## Phase 8 — Hardening & Local Dev Setup

Goal: project runs cleanly from a fresh clone.

- [ ] `docker-compose.yml` for local PostgreSQL instance
- [ ] `.env.example` / `appsettings.Development.json.example` with all required keys documented
- [ ] Global error handling middleware in API (returns RFC 7807 problem details)
- [ ] CORS configured for local dev (Vite port) and production origin
- [ ] Swagger enabled in Development only
- [ ] Verify full flow end-to-end: email in → ticket created → AI draft → agent sends reply → ticket resolved
