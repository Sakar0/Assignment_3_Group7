# Programmer's Log — HIT339 Assignment 3 — Samir's Reservations, Waitlist and Notifications

**Developer:** Samir  
**Assignment:** HIT339 Assignment 3 — Reservations, Waitlist and Notifications  
**Branch:** Samir  
**Remote:** origin/Samir  
**Repository:** https://github.com/Sakar0/Assignment_3_Group7  

---

## Stage 0 — Repository Inspection and Safe Cleanup

### Step 0.1 — Repository and Git State Verification

**Date:** 2026-01-22

**Findings:**

- **Working Directory:** `C:\Users\LENOVO\hit339\Assignment 2` ✓
- **Solution File:** `Assignment 2.slnx` ✓
- **Project File:** `Assignment 2/Assignment 2.csproj` ✓
- **Target Framework:** .NET 10 ✓
- **Current Branch:** `Samir` ✓
- **Remote Configuration:**
  - Origin: `https://github.com/Sakar0/Assignment_3_Group7.git` ✓
- **Branch Status:** 
  - Local `Samir` branch is synced with `origin/Samir`
  - Git status: clean working tree
  - No staged or uncommitted changes
  - No merge or rebase in progress
- **Recent Commits:**
  - `7f7ac3c` (HEAD → Samir, origin/Samir, origin/master): "Add project files."
  - `d0ec2bc`: "Add .gitattributes, .gitignore, and README.md."

**Status:** ✓ SAFE TO PROCEED

---

### Step 0.2 — Audit of Existing Samir Feature Work

**Findings:**

No existing reservation, waitlist, or notification implementation found on the Samir branch. The branch contains only the base project structure.

**Classes Found:**

**Models & Data:**
- `Item.cs` (abstract base class)
- `Book.cs`, `Music.cs`, `Toy.cs` (Item types)
- `Borrower.cs` (borrower information)
- `BorrowRecord.cs` (loan tracking)
- `ErrorViewModel.cs`

**Services:**
- `AdminService.cs` (item management)
- `ReceptionService.cs` (borrower and loan management)
- `ManagerService.cs` (statistics and reporting)
- `SearchService.cs` (item search)
- `DataService.cs` (JSON persistence)

**Controllers:**
- `HomeController.cs`
- `SearchController.cs`

**Views:**
- `Home/Index.cshtml`, `Home/Privacy.cshtml`
- `Search/Index.cshtml`
- `Shared/_Layout.cshtml`, `Error.cshtml`, `_ValidationScriptsPartial.cshtml`

**Configuration:**
- `Program.cs` (MVC startup, sample data initialization)
- `appsettings.json`, `appsettings.Development.json`

**Current State:**
- Application builds successfully: ✓
- No database or Entity Framework configured
- Data persisted via JSON files to `data/` directory
- No user authentication or role system implemented
- No existing reservation, waitlist, or notification functionality

---

### Step 0.3 — Cleanup Requirements

**Analysis:**

The project structure is clean and contains no Samir feature work, experimental code, or broken references. No cleanup is necessary.

**Conclusion:** ✓ NO CLEANUP REQUIRED

---

### Step 0.4 — Safe Cleanup (Not Applicable)

No cleanup work was performed as no issues were identified.

---

### Step 0.5 — Establish Baseline

**Build Status:**
```
Build result: SUCCESSFUL
```

**Baseline Commit:**
- Hash: `7f7ac3c`
- Message: "Add project files."
- Status: Clean, full working tree, ready for implementation

**Test Results:**
No tests configured in the project. Test framework integration will be added during Stage 7.

**Known Constraints:**

1. **No database or Entity Framework:** The project uses in-memory collections (`Item.AllItems`, `Borrower.AllBorrowers`, `BorrowRecord.AllBorrowRecords`) and JSON file persistence via `DataService.cs`.
   - **Impact:** Reservations and notifications must use the same persistence model (in-memory collections + JSON files) unless a major architectural change is approved.
   - **Rationale:** Changing persistence mid-project would break existing modules and require schema design coordination with the team.

2. **No User Authentication System:** The project has no built-in authentication, roles, or user identity.
   - **Impact:** Reservations feature must implement user identification through alternative means (e.g., borrower ID, session, or inline form field).
   - **Rationale:** Adding full ASP.NET Core Identity would be a major undertaking and risks destabilizing existing functionality.

3. **MVC Architecture (not Razor Pages):** Application uses ASP.NET Core MVC with Controllers and Views.
   - **Impact:** UI implementation must follow MVC patterns (controllers, action methods, views).

4. **JSON Persistence Model:** Item, Borrower, and BorrowRecord data are serialized to JSON files.
   - **Impact:** Reservations and Notifications must be serialized using the same `DataService` pattern and custom `JsonConverter` if needed.

5. **No Outer Transaction Control:** Data changes are not wrapped in database transactions.
   - **Impact:** Concurrent operations (e.g., two users trying to reserve the same item simultaneously) rely on in-memory lock-free logic or sequence validation.

---

## Stage 1 — Implementation Plan

### Overview

This implementation will add three interconnected features to the existing Library Management System:

1. **Reservation Model & Persistence:** Entities and data storage for reservations and waitlist entries.
2. **Waitlist & Queue Workflow:** Logic to manage the queue of users waiting for unavailable items.
3. **Notification System:** Persistent in-application notifications tied to reservation and waitlist events.

### Architectural Decisions

**User Identification:**
- Reservations and notifications will be associated with borrower IDs (as in `Borrower.BorrowerId`).
- No authentication system will be added; the web UI will require the user to provide or select their borrower ID.
- Authorization will be enforced server-side: users can view only their own reservations and notifications.

**Persistence:**
- New models (`Reservation`, `WaitlistEntry`, `Notification`) will follow the same in-memory + JSON pattern as existing models.
- Static collections will be created: `Reservation.AllReservations`, `WaitlistEntry.AllWaitlistEntries`, `Notification.AllNotifications`.
- A custom `DataService` extension will handle serialization.

**Concurrency:**
- Lock-free logic: check item status and borrower ID before creating reservations.
- Duplicate prevention via LINQ checks.
- Manual conflict resolution for waitlist processing.

**Notifications:**
- In-application only (no external email or SMS).
- Persisted alongside other data in JSON.
- Accessible via controller and views; read/unread state tracked.

### Planned Implementation Stages

| Stage | Title | Deliverables |
|-------|-------|--------------|
| 1 | Repository Inspection & Design | Plan, baseline, identified files |
| 2 | Reservation Data Model & Persistence | `Reservation.cs`, `DataService` extension, migration logic |
| 3 | Reservation Services & Validation | `ReservationService.cs`, eligibility checks, duplicate prevention |
| 4 | Reservation MVC Interface | `ReservationController.cs`, views, navigation |
| 5 | Waitlist Workflow | `WaitlistEntry.cs`, `WaitlistService.cs`, queue processing |
| 6 | Notification System | `Notification.cs`, `NotificationService.cs`, event hooks |
| 7 | Integration, Security & Testing | Automated tests, security review, full integration test |
| 8 | Final Documentation & Verification | Complete `ProgrammersLog-Samir.md`, README updates, final build & tests |

### Key Files to Create or Modify

**New Models:**
- `Assignment 2/LibraryManagement/Reservation.cs`
- `Assignment 2/LibraryManagement/WaitlistEntry.cs`
- `Assignment 2/LibraryManagement/Notification.cs`

**New Services:**
- `Assignment 2/LibraryManagement/ReservationService.cs`
- `Assignment 2/LibraryManagement/WaitlistService.cs`
- `Assignment 2/LibraryManagement/NotificationService.cs`

**New Controller:**
- `Assignment 2/Controllers/ReservationController.cs`

**New Views:**
- `Assignment 2/Views/Reservation/Index.cshtml` (list user reservations)
- `Assignment 2/Views/Reservation/Create.cshtml` (create reservation form)
- `Assignment 2/Views/Reservation/Details.cshtml` (view reservation details)
- `Assignment 2/Views/Notification/Index.cshtml` (list notifications)

**Modified:**
- `Assignment 2/Program.cs` (register services, initialize data)
- `Assignment 2/LibraryManagement/DataService.cs` (add reservation/notification persistence)
- `Assignment 2/LibraryManagement/ReceptionService.cs` (hook item return to waitlist processing)
- `Assignment 2/Views/Shared/_Layout.cshtml` (add navigation links)

**Tests:**
- `Assignment 2.Tests/ReservationServiceTests.cs`
- `Assignment 2.Tests/WaitlistServiceTests.cs`
- `Assignment 2.Tests/NotificationServiceTests.cs`

### Risks and Dependencies

| Risk | Mitigation |
|------|------------|
| **Persistence Layer Complexity** | JSON serialization of complex nested lists (Reservation → Item, Borrower). Implement custom converter if needed. |
| **Concurrency Issues** | No database locking in JSON model. Validate state before critical operations; handle edge cases (stale reads, duplicate offers). |
| **User Identification** | No authentication system. Rely on borrower ID submission + server-side authorization checks. Users must provide their ID or select from a list. |
| **Existing Feature Breakage** | Changes to `ReceptionService.cs` (return flow) and `Program.cs` (bootstrapping). Verify borrowing/return workflows remain functional. |
| **Notification Display** | Ensure notifications appear in user-accessible views without breaking layout. Test on different screen sizes. |

---

## Implementation Status

### Completed Stages

- ✓ Stage 0.1-0.5: Repository inspection, baseline established
- ✓ Stage 1: Created implementation plan
- ✓ Stage 2: Reservation data models and persistence
- ✓ Stage 3: Reservation services and validation
- **Stage 4: IN PROGRESS** — Reservation MVC interface

### Current Work (Stage 4)

**Completed:**
- Created `ReservationController.cs` with actions:
  - Index(borrowerId): List/filter user reservations
  - Create GET/POST: Form to create new reservations
  - Details(reservationId, borrowerId): View reservation details (with auth check)
  - Cancel POST: Cancel pending reservations (with owner-only auth)
- Created Razor views:
  - `Index.cshtml`: Lists borrower's reservations with status badges and actions
  - `Create.cshtml`: Form for creating reservations with borrower/item selection
  - `Details.cshtml`: Full reservation details with borrower info, item info, cancel option
- Updated `_Layout.cshtml`: Added "My Reservations" navigation link
- Build verified: ✓ SUCCESSFUL

**Key Features:**
- ✓ User-friendly borrower selection
- ✓ Validation error messages
- ✓ Authorization checks (can only view/cancel own reservations)
- ✓ Success/error feedback via TempData
- ✓ Bootstrap styling with badges and responsive layout
- ✓ Anti-forgery tokens on forms

**Pending:**
- Stage 4 final commit
- Stages 5-8 implementation

### Upcoming Stages

- **Stage 5:** Waitlist workflow
- **Stage 6:** Notification system
- **Stage 7:** Integration, security and testing
- **Stage 8:** Final documentation and verification

---

## Commit History (Samir Branch)

### Baseline Commits (Before Implementation)

| Hash | Branch | Message |
|------|--------|---------|
| `7f7ac3c` | Samir, origin/Samir | Add project files. |
| `d0ec2bc` | (parent) | Add .gitattributes, .gitignore, and README.md. |

### Implementation Commits (To Be Added)

| Hash | Stage | Message | Status |
|------|-------|---------|--------|
| `b3b8ebe` | 1 | `docs: document reservation and notification implementation plan` | ✓ Pushed |
| `ed49726` | 2 | `feat: add reservation models and persistence` | ✓ Pushed |
| `546eece` | 3 | `feat: implement reservation workflow and validation` | ✓ Pushed |
| (pending) | 4 | `feat: add reservation management interface` | Pending |
| (pending) | 5 | `feat: implement item waitlist and queue processing` | Pending |
| (pending) | 6 | `feat: add reservation and waitlist notifications` | Pending |
| (pending) | 7 | `test: verify reservation waitlist and notification workflows` | Pending |
| (pending) | 8 | `docs: document and verify reservation feature` | Pending |

---

## Notes for Stage 2 and Beyond

1. **Reservation Model Design:**
   - Fields: ReservationId (unique), BorrowerId, ItemLibraryCode, ReservationDate, Status (Pending, Fulfilled, Cancelled), ExpiryDate.
   - Relationships: Link to Item (by LibraryCode) and Borrower (by BorrowerId).
   - Statuses: Define clear transitions (Pending → Fulfilled or Cancelled).

2. **WaitlistEntry Model Design:**
   - Fields: WaitlistEntryId, BorrowerId, ItemLibraryCode, QueuePosition, CreatedDate, Status (Waiting, Accepted, Declined, Expired).
   - Queue ordering: By CreatedDate (FIFO).

3. **Notification Model Design:**
   - Fields: NotificationId, RecipientBorrowerId, Message, Type (ReservationCreated, ReservationFulfilled, OfferMade, OfferExpired, etc.), CreatedDate, IsRead.
   - Recipient filtering: Users can view only their own notifications.

4. **Integration Points:**
   - **Item Return:** When `ReceptionService.ReturnItem()` is called, trigger `WaitlistService.ProcessNextWaitlistEntry()`.
   - **Reservation Cancellation:** Update reservation status and remove from waitlist if applicable.
   - **Offer Expiry:** Implement time-based expiry logic; mark as expired if not accepted within X days.

---

## Known Limitations and Future Improvements

1. **Email Notifications:** Not implemented. All notifications are in-application only.
2. **Offer Expiration:** Can be hardcoded (e.g., 7 days) or made configurable via `appsettings.json`.
3. **Reservation Eligibility:** Currently defined as "item is unavailable (status != Available)". Can be extended to check item type, borrower history, etc.
4. **Concurrent Access:** No distributed locking. Works correctly for single-instance web app; would require additional logic for multi-instance deployment.

