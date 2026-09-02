# PatchlabTicketing

Support ticket dashboard for Patchlab. Reads and displays tickets from the shared `Patchlab.Tickets` SQL table, the same table `PatchlabWhatsAppBot` writes to.

## Stack

- **API**: ASP.NET Core Web API, Dapper for data access
- **Client**: React (Vite), polling-based refresh
- **Database**: SQL Server, `Patchlab` database, `Tickets` table (shared with `PatchlabWhatsAppBot`, not owned by this repo)

## Architecture

No HTTP contract between this app and `PatchlabWhatsAppBot`. Both read/write the same SQL table directly, the database is the interface.

## Production Infrastructure

**As of 2026-09-02**, this app runs on a new production server, `MULTIPOINT`, migrated from the old shared multi-client production box. Confirmed working there with real production data: `Customers`, `Tickets`, `DeletedTickets`, `TicketComments`, `TicketFeedback`, and `TicketPhotos` rows were migrated via manually-run `INSERT` scripts built from the old server's data export, and the ticket photo files themselves were copied over so `TicketPhotos/` on `MULTIPOINT` matches what those rows point at.

`TicketPhotosRootPath` needs to be set to wherever that copied `TicketPhotos/` folder actually lives on `MULTIPOINT` — this key is documented in `appsettings.Example.json` and in "Ticket photos" below (merged via [#14](https://github.com/RealArbitrary/PatchlabTicketing/pull/14)).

### CI/CD — NOT YET TESTED on new server

`.github/workflows/deploy.yml` has not been validated against `MULTIPOINT` yet — it was written and last confirmed working against the old server, and nothing about it has been re-verified against the new one. On the first real deploy to `MULTIPOINT`, check:

- The NSSM service actually restarts cleanly (the poll/retry logic that absorbs the `SERVICE_START_PENDING` race assumes `nssm status` behaves the same way here as it did on the old server's NSSM install — confirm that holds).
- Any paths the pipeline or app config reference that assume the old server's install directory layout still resolve correctly on `MULTIPOINT` — particularly `TicketPhotosRootPath` and the publish output path (`C:\PatchlabTicketing\publish`).
- The SQL connection string in `appsettings.json` still points at a valid, reachable database instance from `MULTIPOINT` — the old server and `MULTIPOINT` are not guaranteed to see the same instance name or network path.

### Migration history

Migrated off the old shared multi-client production box. That server's `CloudflaredTunnel` setup was replicated on `MULTIPOINT` to keep external access working the same way. `PatchlabNgrokSync` was **not** replicated — it was a stale leftover on the old server with no corresponding repo, and was intentionally left behind rather than carried over.

## Ticket photos

`PatchlabWhatsAppBot` writes uploaded ticket photos to disk under `TicketPhotos/yyyy/MM/dd/<guid>.<ext>`, relative to wherever it's installed on that machine — only the relative path is stored in SQL. This API serves those files as static content (under `/photos`) so the client can display and link to them, which means `appsettings.json`'s `TicketPhotosRootPath` must be set to the **absolute** path of that `TicketPhotos/` folder on whatever machine this API runs on.

There's no safe default for this — it depends entirely on where `PatchlabWhatsAppBot` happens to be installed on that specific server, which varies per deploy. See `appsettings.Example.json` for the placeholder to fill in.

## Project structure

PatchlabTicketing/
├── PatchlabTicketing.Api/ ASP.NET Core Web API
└── patchlabticketing.client/ React dashboard (Vite)

## Running locally

**API:**

1. Open `PatchlabTicketing.Api/PatchlabTicketing.Api.slnx` in Visual Studio
2. Confirm `appsettings.json` connection string points at your local or server SQL instance, and set `TicketPhotosRootPath` to the real absolute path on this machine (see "Ticket photos" below)
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
