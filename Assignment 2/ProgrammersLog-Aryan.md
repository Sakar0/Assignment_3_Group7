# Programmer's Log — Aryan

**Feature:** Self-Service Library Kiosk + Secure Read-Only REST API
**Branch:** `Aryan-branch` (tracks `origin/Aryan-branch`)
**Project:** Library Management System (Assignment 3, Group 7)
**Language/Framework:** C# 14 / ASP.NET Core MVC on .NET 10

---

## Stage 1 — Repository Inspection and Design (2026-10-15)

### Initial repository state (verified before any changes)

| Check | Result |
|---|---|
| Working directory | `C:\Users\Ritik\OneDrive\Documents\GitHub\Assignment_3_Group7` |
| Solution file | `Assignment 2.slnx` — single project: `Assignment 2/Assignment 2.csproj` |
| Project file | `Assignment 2.csproj`, `Microsoft.NET.Sdk.Web`, `net10.0`, nullable + implicit usings enabled |
| Git remote | `origin` → `https://github.com/Sakar0/Assignment_3_Group7.git` |
| Remote branches | `origin/master`, `origin/Aryan-branch`, `origin/Samir`, `origin/Rohan-Branch`, `origin/integration-group7` |
| Branch state | No local `Aryan-branch` existed; remote `origin/Aryan-branch` existed at `7f7ac3c` ("Add project files", same commit as `master`). Created local tracking branch with `git checkout Aryan-branch` — exact remote history, no replacement branch, no work done on `master`. |
| Working tree | Clean before changes |
| Baseline build | `dotnet build "Assignment 2.slnx"` → **Build succeeded, 0 warnings, 0 errors** (.NET SDK 10.0.302) |

### Existing architecture (as inspected — not assumed)

- **Entry point:** `LibraryManagementConsole` (console menu); the web app starts via `WebAppStartup.RunWebApp(args)` when `--web` is passed.
- **Persistence:** JSON files in a `data/` folder via static `DataService` (`items.json`, `borrowers.json`, `borrow_records.json`) with a polymorphic `ItemJsonConverter`. **There is no Entity Framework Core in this codebase** — data lives in process-wide static collections (`Item.AllItems`, `Borrower.AllBorrowers`, `BorrowRecord.AllBorrowRecords`). The assignment brief mentioned EF Core; the actual code differs, so all features below are designed for the real persistence mechanism.
- **Domain model:**
  - `Item` (abstract): `Name`, `Description`, `LibraryCode`, `Status` (`Available` / `Borrowed` / `Damaged` / `Destroy`).
  - `Book` (+`Author`, `Genre`), `Music` (+`Artist`, `Year`), `Toy` (+`Type`, `AgeGroup`).
  - `Borrower`: `BorrowerId`, `Name`, `Phone`, `Email`.
  - `BorrowRecord`: `LibraryCode`, `BorrowerId`, `BorrowDate`, `ReturnDate?`, `FineAmount`; 14-day loan, $1/day overdue fine via `CalculateFine()` / `IsOverdue()`.
- **Business services:**
  - `ReceptionService` (instance): `CreateBorrower`, `FindBorrowerById`, `SearchBorrowerByName`, `UpdateBorrower`, `DeleteBorrower`, `BorrowItem(libraryCode, borrowerId)`, `ReturnItem(libraryCode)`, `GetActiveBorrows`, `GetBorrowerHistory`. `BorrowItem` already enforces: item exists, borrower exists, no active duplicate borrow of the same item, and `Status == "Available"`. `ReturnItem` sets `ReturnDate`, computes the fine, and restores `Available` status.
  - `SearchService` (static): `SearchItems(name, code, type)`, `FindByLibraryCode`, `GetItemsByStatus`, `GetItemsByType`, name/code suggestions, counts.
  - `AdminService`: item CRUD (create/edit/delete books, music, toys).
  - `ManagerService`: statistics/reports.
- **Web layer:** `HomeController` (Index/Privacy/Error), `SearchController` (public catalogue search page + two JSON autocomplete endpoints). Razor views with Bootstrap 5 + bootstrap-icons classes, `_Layout.cshtml` navbar (Home, Privacy).
- **Middleware (`WebAppStartup`):** `UseRouting` → `UseAuthorization` → `MapStaticAssets` → conventional route `{controller=Home}/{action=Index}/{id?}`. Exception handler `/Home/Error` + HSTS outside Development.
- **Authentication/authorization:** **None configured.** No Identity, no auth scheme, no `[Authorize]`, no passwords anywhere. `UseAuthorization()` runs with no authentication middleware. Borrowers are identified solely by their `BorrowerId` string (same mechanism the reception desk workflow uses).
- **API conventions:** No API controllers, no versioning, no Swagger/OpenAPI package, no test project in the solution.
- **Features listed in the brief as "existing" (reservations, notifications, transfers, CSV import, Analytics, Export) do NOT exist on this branch** — they live on other members' branches / `integration-group7`. Nothing on `Aryan-branch` implements them, so my changes cannot break them here; final integration is the team's responsibility later.

### Constraints derived from the inspection

1. Do not bypass `ReceptionService` business rules — kiosk must reuse `BorrowItem`/`ReturnItem`.
2. Do not add unnecessary packages (no EF Core, no Swagger package unless justified — will document endpoints in the log/README instead and provide an `.http` test file).
3. Static in-memory collections are shared process-wide → kiosk/API automatically share state with the existing pages. Saves require `DataService.SaveAllData()`; the existing web flow never persists borrow operations to disk automatically, so the kiosk will persist after successful checkout/return (consistent with the console app's explicit-save convention) and this will be documented.
4. No user credentials exist → kiosk identification can only be by `BorrowerId` (the existing mechanism). The read-only API will expose catalogue data only — never borrower PII.
5. `Item.Status` uses string values `Available`/`Borrowed`/`Damaged`/`Destroy`; availability check = `Status == "Available"`.

### Assigned responsibilities

A library self-service kiosk (MVC UI) and a secure, read-only REST API, integrated into this existing application on `Aryan-branch`.

### Planned stages (8-stage workflow)

1. **Stage 1 (this entry):** inspection + design log. Commit `docs: document kiosk and read-only API implementation plan`.
2. **Stage 2:** API foundation — `CatalogueController` (`api/catalogue`), `ItemDto`, list + detail GET endpoints.
3. **Stage 3:** API validation (400/404), pagination caps, error shape, endpoint documentation.
4. **Stage 4:** Kiosk foundation — landing page, catalogue browse/search, item details, kiosk layout.
5. **Stage 5:** Kiosk checkout/return via `ReceptionService`.
6. **Stage 6:** Kiosk validation, user feedback, accessibility.
7. **Stage 7:** Automated tests + full regression.
8. **Stage 8:** Final documentation + verification.

### Kiosk workflow design

- **Controller:** `KioskController` (MVC, conventional routing → `/Kiosk/...`).
- **Pages:**
  - `GET /Kiosk` — landing page: large touch-friendly actions (Browse Catalogue, Check Out, Return, My Loans).
  - `GET /Kiosk/Catalogue?searchName=&searchType=&searchCode=` — browse/search, reusing `SearchService.SearchItems`; results show availability badge.
  - `GET /Kiosk/Item/{code}` — item details + availability; "Check Out" button only when `Available`.
  - `GET /Kiosk/Checkout/{code}` → `POST /Kiosk/Checkout` — borrower enters **their own** `BorrowerId`; server re-validates via `ReceptionService.BorrowItem`. Re-check availability immediately before checkout to prevent duplicate/conflicting checkouts.
  - `GET /Kiosk/Return` → `POST /Kiosk/Return` — scan/type a library code; server uses `ReceptionService.ReturnItem`; shows fine (if any). Returns are item-keyed in the existing domain (no borrower needed, matching reception desk rules).
  - `GET /Kiosk/MyLoans?borrowerId=` — borrower's active loans via `ReceptionService.GetBorrowerHistory`.
- **Identification:** `BorrowerId` only (no passwords exist in the system — documented limitation). The kiosk never exposes borrower lists; ID must be known and typed by the user.
- **Not included (by design):** creating borrowers, editing items, admin/manager functions, borrowing on behalf of arbitrary third parties — the kiosk is self-service only.
- **Feedback:** `TempData` success/error banners; invalid codes/IDs show clear messages; unavailable items cannot be checked out (button hidden + server-side re-check).

### API resource and endpoint plan

- **Controller:** `Api/CatalogueController`, attribute-routed, `[ApiController]`, route prefix `api/catalogue`. GET-only.
- **Endpoints:**
  - `GET /api/catalogue` — paged list. Query: `name` (partial), `type` (`Book|Music|Toy`), `status`, `page` (≥1, default 1), `pageSize` (1–50, default 12). Returns `200` with `{ items: [...ItemDto], page, pageSize, totalItems, totalPages }`; `400` for invalid params.
  - `GET /api/catalogue/{libraryCode}` — single item. `200` with `ItemDto`; `400` for empty/malformed code; `404` when not found.
- **DTO (`ItemDto`):** `libraryCode`, `name`, `description`, `type`, `status`, `isAvailable` + type-specific: `author`/`genre` (Book), `artist`/`year` (Music), `toyType`/`ageGroup` (Toy). No internal entities, no borrower data, no fines.
- **Errors:** RFC 7807 `ProblemDetails` (built-in, no new packages). No stack traces/secrets.
- **Read-only guarantee:** only `[HttpGet]` actions; controller depends only on read paths (`SearchService`, `Item.AllItems` lookups); no write service is referenced. Stage 7 test asserts a `POST/PUT/DELETE` to the route is rejected (405) and that repeated GETs do not mutate state.
- **Local testing:** `Assignment 2/KioskApi.http` request file + log documentation.

### Security and authorization considerations

- The app has **no authentication infrastructure**; adding Identity would violate "avoid unnecessary dependencies" and duplicate nothing that exists. Decisions:
  - **API:** anonymous but strictly read-only catalogue data (same data the existing public `/Search` page already exposes). No borrower PII, no secrets, no config.
  - **Kiosk:** operations are gated per-transaction by `BorrowerId` (the system's only identity mechanism). The kiosk cannot impersonate staff: no admin/reception/manager actions are exposed in kiosk UI or routes.
- Server-side re-validation of every checkout/return through `ReceptionService` (item exists, borrower exists, item available, no duplicate active borrow).
- No stack traces in API errors; generic 500 message.
- Nothing secrets-related is logged or committed.

### Files/components identified for change (later stages)

- New: `Controllers/Api/CatalogueController.cs`, `Models/Api/ItemDto.cs`, `Controllers/KioskController.cs`, `Views/Kiosk/*`, `wwwroot/css/kiosk.css` (or site.css addition), `KioskApi.http`, test project (Stage 7).
- Edit: `Views/Shared/_Layout.cshtml` (add Kiosk nav link only), `ProgrammersLog-Aryan.md` (this file).
- No changes planned to: `LibraryManagement/*` services, existing controllers/views, `Program.cs` (attribute-routed API controllers need no new routing registration).

### Test requirements identified

- Baseline build verified green (see above). No test project exists — Stage 7 will add an xUnit project (`Assignment 2.Tests`) referencing the app, testing: catalogue list/detail, 400/404 handling, read-only guarantee, `ReceptionService` checkout rules via the kiosk path, duplicate checkout rejection, unavailable-item rejection, and regression of existing search behaviour. No claim of "tests passed" will be made without actual runs.

---
