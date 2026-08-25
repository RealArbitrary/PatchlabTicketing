# PatchlabTicketing

Support ticket dashboard for Patchlab. Reads and displays tickets from the shared `Patchlab.Tickets` SQL table, the same table `PatchlabWhatsAppBot` writes to.

## Stack

- **API**: ASP.NET Core Web API, Dapper for data access
- **Client**: React (Vite), polling-based refresh
- **Database**: SQL Server, `Patchlab` database, `Tickets` table (shared with `PatchlabWhatsAppBot`, not owned by this repo)

## Architecture

No HTTP contract between this app and `PatchlabWhatsAppBot`. Both read/write the same SQL table directly, the database is the interface.

This repo is Dapper-only — no EF Core, no migrations. It does not own schema for any table in the shared `Patchlab` database, including tables its own features depend on (e.g. `DeletedTickets`, the hard-delete archive table). Schema for the whole shared database — plus any operational tooling on it, such as the `dbo.PurgeDeletedTickets` stored proc and its SQL Agent job — is owned by `PatchlabWhatsAppBot`, not this repo.

## Project structure

PatchlabTicketing/
├── PatchlabTicketing.Api/ ASP.NET Core Web API
└── patchlabticketing.client/ React dashboard (Vite)

## Running locally

**API:**

1. Open `PatchlabTicketing.Api/PatchlabTicketing.Api.slnx` in Visual Studio
2. Confirm `appsettings.json` connection string points at your local or server SQL instance
3. Run, should open Swagger at `https://localhost:7168/swagger`

**Client:**

1. Open `patchlabticketing.client/` in VS Code (or any editor)
2. `npm install`
3. `npm run dev`
4. Open `http://localhost:5173`

Both need to be running at the same time for the dashboard to load ticket data.

## Status

Tickets can be viewed and closed, and each ticket row expands inline to show its feedback and comment thread (with the ability to add new comments). A separate Error Logs view is also available from the nav. No authentication yet, and conversation takeover is not implemented (the "Conversations" nav entry is present but disabled).

Status values: `Open` (green), `Closed` (blue). More states may be added once the manual chat takeover flow is designed.

The API exposes four resources: tickets, ticket comments, ticket feedback, and error logs.

## Roadmap

- Manual takeover / conversation view page
- Ticket reopening
- Authentication
