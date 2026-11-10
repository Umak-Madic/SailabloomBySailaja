# Sailabloom by Sailaja — ASP.NET Core

*Every Garden Begins with Love.*

A layered ASP.NET Core 8 solution for Sailaja's home plant store: a Razor Pages
storefront, a thin Web API, and an Entity Framework Core data layer whose
provider is **plug-and-play** (InMemory / SQLite / SQL Server by configuration).

This is the structured .NET version of the hosted Sailabloom prototype — all of
its owner-approved business rules are ported into `Sailabloom.Application`.

## Architecture

```
                 ┌──────────────────────────────┐
                 │        Sailabloom.Web        │  Razor Pages storefront
                 │  Shop · Cart · Checkout ·    │  + Identity-protected /Admin
                 │  Guarantee · Admin area      │
                 └──────────────┬───────────────┘
                 ┌──────────────┴───────────────┐
                 │        Sailabloom.Api        │  Thin JSON controllers
                 │  plants · orders · reports   │  (Swagger in Development)
                 └──────────────┬───────────────┘
                                │  both hosts use only
                                ▼  Application services + interfaces
                 ┌──────────────────────────────┐
                 │    Sailabloom.Application    │  BUSINESS RULES live here
                 │  CatalogService              │  - atomic stock claim
                 │  OrderService                │  - payments / fulfillment
                 │  ReportService               │  - refunds / replace / cancel
                 │  IPlantRepository, ...       │  - NET sales reporting
                 │  IUnitOfWork                 │
                 └──────────────┬───────────────┘
                                │  implemented by
                                ▼
                 ┌──────────────────────────────┐
                 │   Sailabloom.Infrastructure  │  DB LAYER
                 │  SailabloomDbContext (EF)    │  provider switch:
                 │  Repositories · UnitOfWork   │  InMemory | Sqlite | SqlServer
                 │  DbSeeder (8 starter plants) │
                 └──────────────┬───────────────┘
                                │  depends on
                                ▼
                 ┌──────────────────────────────┐
                 │      Sailabloom.Domain       │  Entities + enums. No deps.
                 │  Plant · Order · OrderItem   │
                 │  Refund · Replacement ·      │
                 │  IssueReport                 │
                 └──────────────────────────────┘

   Sailabloom.Tests — xUnit tests for the stock rules (InMemory provider).
```

| Project | What it is |
| --- | --- |
| `src/Sailabloom.Domain` | Plain entities and enums. References nothing. |
| `src/Sailabloom.Application` | Services (the business rules) + repository interfaces + the unit-of-work abstraction. References Domain only — no EF Core in this layer. |
| `src/Sailabloom.Infrastructure` | EF Core `SailabloomDbContext` (also hosts the ASP.NET Core Identity tables), entity configurations, repository implementations, transactional unit of work, catalog seeder, and the DI extension that picks the EF provider from configuration. |
| `src/Sailabloom.Api` | Thin Web API controllers over the Application services: `GET /api/plants`, `POST /api/orders`, admin plant CRUD at `/api/admin/plants`, reports at `/api/reports/*`. |
| `src/Sailabloom.Web` | The Razor Pages storefront and the Identity-protected Admin area. |
| `tests/Sailabloom.Tests` | xUnit tests proving the stock rules. |

## Run it (Visual Studio 2026)

1. Open `Sailabloom.sln`.
2. Set **Sailabloom.Web** as the startup project and press F5.
   - Default profile: `https://localhost:7241` (http fallback `http://localhost:5241`).
   - On first run the catalog is seeded with the 8 starter plants and the admin
     account is created from configuration (see below).
3. (Optional) Run **Sailabloom.Api** the same way for the JSON API + Swagger
   (`https://localhost:7242/swagger` in Development).

From the command line instead:

```powershell
dotnet run --project src/Sailabloom.Web
dotnet run --project src/Sailabloom.Api
dotnet test
```

## Switching the database provider — config only

The provider is chosen by the `DataProvider` key in each host's
`appsettings.json`. All three EF Core providers are already referenced by
`Sailabloom.Infrastructure`, so **no code or package changes** are needed:

```jsonc
// InMemory (default) — zero setup, data resets on restart. For testing.
{ "DataProvider": "InMemory" }

// SQLite — a real file database, no server needed.
{
  "DataProvider": "Sqlite",
  "ConnectionStrings": { "Sailabloom": "Data Source=sailabloom.db" }
}

// SQL Server — LocalDB shown; point it at any SQL Server instance.
{
  "DataProvider": "SqlServer",
  "ConnectionStrings": {
    "Sailabloom": "Server=(localdb)\\mssqllocaldb;Database=SailabloomBySailaja;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Notes:

- The schema is created with `Database.EnsureCreated()` at startup (in
  `DbSeeder`) so the dev providers work with zero ceremony. When you move to
  SQL Server for production, switch to EF Core **migrations**
  (`dotnet ef migrations add InitialCreate` against the Infrastructure
  project) and remove the `EnsureCreated` call.
- Each host process gets its **own** InMemory database — with `InMemory`, the
  Web and API don't share data. Use `Sqlite` when you want them to share one
  local database file while testing.

## Admin account

The `/Admin` folder is protected by ASP.NET Core Identity + the **Admin**
role (`AuthorizeFolder` in `Program.cs`). At startup, `IdentitySeeder`
creates the role and the owner account from configuration:

```jsonc
// src/Sailabloom.Web/appsettings.json — CHANGE-ME placeholders
"AdminUser": {
  "Email": "admin@sailabloom.local",
  "Password": "CHANGE-ME-Admin-123!"
}
```

Set your real values **without committing them**, e.g. with user secrets:

```powershell
cd src/Sailabloom.Web
dotnet user-secrets set "AdminUser:Email" "you@example.com"
dotnet user-secrets set "AdminUser:Password" "Your-Strong-Password-1!"
```

…or with environment variables `AdminUser__Email` / `AdminUser__Password`.
Sign in at `/Account/Login`. To change the password later, update the secret
and delete the old user from the database (the seeder only creates the account
when it doesn't exist), or add a change-password page.

## Feature checklist (ported from the live store)

- [x] Catalog with search + category filters; starter catalog of 8 plants, fully editable in Admin
- [x] Session cart, quantities capped at current stock
- [x] Checkout payments — **Card** (demo; only brand + last 4 stored, full number never saved), **Zelle** to (281) 777-2499 (order marked *payment pending verification*), **Cash** (pickup only, *due at pickup*)
- [x] Fulfillment — Standard shipping **$15** (default), pre-paid customer label **$0**, pickup at home **$0**; address required only for shipped orders; cash forces pickup — enforced on the server, not just in the browser
- [x] **Atomic, race-safe stock claim at checkout**: carts never reserve stock; duplicate cart lines are merged; every line is validated, then decrements + the order save happen in one transaction (serializable on relational providers, with stock as an EF concurrency token). Any shortfall rejects the whole order and changes no stock — first come, first served
- [x] **Arrive-Alive Guarantee** page: report within 48 hours with a photo → free replacement or full refund including shipping; dead plant never returned
- [x] Account-free **Report a problem** form: order number + checkout email must match (one generic error otherwise — no order details leak), optional photo upload
- [x] Admin **Orders**: status lifecycle (Placed → Shipped → Delivered, Issue reported, Replaced / Refunded / Cancelled)
- [x] **Refunds**: full or partial, reason required, cumulative refunds can never exceed the order total; card refunds recorded to the original card (demo), Zelle refunds two-step (*send manually* → marked sent), cash refunded in cash; **no restock unless explicitly ticked**, and at most once per order
- [x] **Replacements**: $0, claimed from stock with the same rules as checkout, blocked with a refund suggestion when stock is short; never counted as revenue
- [x] **Cancel**: unshipped orders only, auto-restocks and records any refund still owed
- [x] **Sales Reports — inside Admin only** (`/Admin/Reports`): Gross / Total refunds / **NET** headline numbers, month-wise NET chart (server-rendered SVG), category share; *no Sales Reports link exists in the public banner*
- [x] Public banner: Home, Shop, Guarantee, Cart (count badge), Admin — icon next to every item, deep-green botanical theme
- [x] Tests for the stock rules (`dotnet test`)

## Known simplifications in this scaffold

- The API's admin endpoints (`/api/admin/plants`) are **not authenticated** in
  this development scaffold — the Razor Pages Admin area is the protected one.
  Add JWT/cookie auth with an Admin policy before exposing the API publicly.
- Card checkout is a **demo** (as in the live store): no processor is
  connected and no real money moves.
- Seed plant photos are placeholder Unsplash URLs — replace them with
  Sailaja's real photos in Admin.

## CI

`.github/workflows/build.yml` runs on every push: restore, `dotnet build`
(Release), and `dotnet test` on .NET 8 — so the first compile check happens
in GitHub Actions even before the solution is opened locally.
