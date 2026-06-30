## Problem

Our team receives a significant volume of support emails each day. The current process requires agents to manually review, categorize, and reply to every ticket, resulting in slow response times and impersonal, templated answers.

## Solution

An AI-assisted ticket system that ingests support emails from Gmail, classifies and summarizes each one, then drafts a reply using a hardcoded knowledge base. A human agent reviews, edits, and sends every response. The MVP validates the core loop end-to-end.

## MVP Core Loop

1. **Ingest** — Poll Gmail API every ~60s, create a ticket for each new email
2. **Classify & Summarize** — AI categorizes the ticket and writes a short summary
3. **Draft Reply** — AI generates a suggested reply from a hardcoded in-code knowledge base
4. **Review & Send** — Agent sees the queue, edits the draft, sends via Gmail API

## Ticket Behavior

- **Threading**: Reply in the same Gmail thread → reopen existing ticket. New email → new ticket.
- **States**: Open → Resolved (simple two-state lifecycle)

## Auth

- Admin account seeded at deploy
- Admin creates agent accounts (no self-registration)
- Roles: Admin, Agent

## Deferred (post-MVP)

- Knowledge base management UI
- User management UI
- Dashboard
- Audit trail
- Auto-acknowledgement emails
- Complex ticket states (In Progress, Awaiting Customer, etc.)
- Email forwarding / SendGrid integration
- SLA / prioritization
- Notifications

## Tech Stack

TBD
