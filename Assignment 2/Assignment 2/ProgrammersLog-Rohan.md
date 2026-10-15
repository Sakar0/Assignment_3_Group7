# Programmer's Log - Rohan Branch
## Assignment 2: Library Management System - Admin Bulk Import, Manager Analytics, and Report Exports

**Developer:** Rohan  
**Branch:** Rohan-Branch  
**Assigned Features:**
1. Admin bulk catalogue importer (CSV or external metadata provider)
2. Manager analytics and reporting dashboard
3. Export manager reports to CSV and PDF

---

## Stage 1 – Project Inspection and Analysis

**Date:** 2026  
**Status:** COMPLETE  
**Build Result:** ✅ Successful

## Stage 2 – Plan the Importer and Analytics Design

**Date:** 2026  
**Status:** COMPLETE  
**Build Result:** ✅ Successful (no changes, verification only)

### Project Overview

The existing project is an **ASP.NET Core MVC library management system** (.NET 10) with:
- **Console application** (primary) for staff (Admin, Reception, Manager roles)
- **Web application** (secondary) for public catalogue search
- **JSON-based persistence** for items, borrowers, and borrow records
- **Polymorphic item types**: Book, Music, Toy with role-based access

### Key Findings

#### 1. **Item Model and Inheritance Structure**
- **Base Class:** `Item` (abstract)
  - Common properties: `Name`, `Description`, `LibraryCode`, `Status`
  - Static collection: `Item.AllItems` (shared across console and web)
  - Status values: "Available", "Borrowed", "Damaged", "Destroy"

- **Subclasses:**
  - `Book`: Adds `Author` (string), `Genre` (string)
  - `Music`: Adds `Artist` (string), `Year` (int)
  - `Toy`: Adds `Type` (string), `AgeGroup` (string)

**CSV Importer Design Implication:** CSV columns must map to these specific properties per item type.

#### 2. **Existing Catalogue and Search Functionality**
- **SearchController**: Provides public read-only search by Name, LibraryCode, or Type (Book/Music/Toy)
- **Search Features:**
  - Case-insensitive partial match by name
  - Exact match by library code
  - Filtering by item type
  - Alphabetical sorting
- **Data Source:** Static `Item.AllItems` collection (shared with console)

**Importer Integration Point:** New imported items must integrate into `Item.AllItems` to be immediately searchable.

#### 3. **Administrator Functionality**
- **AdminService** provides CRUD operations:
  - `CreateBook()`, `CreateMusic()`, `CreateToy()` – validates and adds items
  - Duplicate library code detection
  - Status validation (enforced by Item base class)
  - Item search, edit, and delete operations
  - Display all items

- **No existing admin UI** in web application – console-only currently

**Importer Design Implication:** Bulk import should use AdminService methods where possible to maintain consistency and validation rules.

#### 4. **Data Persistence Mechanism**
- **DataService** (static class) handles all I/O:
  - **Files:** `data/items.json`, `data/borrowers.json`, `data/borrow_records.json`
  - **JSON Format:** Uses System.Text.Json with custom converters
  - **Custom Converter:** `ItemJsonConverter` uses `$type` discriminator for polymorphic serialization
  - **Load Process:** Deserializes JSON → creates appropriate Item subclass instances
  - **Save Process:** Serializes Item objects (with $type marker) → JSON file

**Importer Reliability:** After bulk insert, imported items must be persisted via `DataService.SaveAllData()` to survive application restart.

#### 5. **Borrowing and Analytics Data**
- **Borrower Class:**
  - Properties: `BorrowerId`, `Name`, `Phone`, `Email`
  - Static collection: `Borrower.AllBorrowers`

- **BorrowRecord Class:**
  - Properties: `LibraryCode`, `BorrowerId`, `BorrowDate`, `ReturnDate`, `FineAmount`
  - Static collection: `BorrowRecord.AllBorrowRecords`
  - Methods: `GetDaysBorrowed()`, `IsOverdue()`, `CalculateFine()` (overdue = >14 days, $1/day)

**Analytics Design:** Dashboard can calculate:
- Total items by type
- Items by status distribution
- Borrowing activity (items currently borrowed)
- Overdue items and fine totals
- Active borrowers, return rates

#### 6. **Manager Functionality**
- **ManagerService** exists in codebase but **no web interface** for managers
- Existing methods likely include statistics and reporting (console-based)

**Manager Dashboard Opportunity:** New web-based dashboard can expose existing ManagerService calculations.

#### 7. **Web Application Architecture**
- **Startup:** `Program.cs` → `WebAppStartup.RunWebApp()` (invoked with `dotnet run -- web`)
- **Middleware:** MVC with Controllers and Razor Views
- **Navigation:** Minimal layout in `Views/Shared/_Layout.cshtml` (only Home, Privacy links)
- **Authentication:** No role-based authorization currently (open access)
- **Data Flow:** Shared static collections (Item.AllItems, Borrower.AllBorrowers, BorrowRecord.AllBorrowRecords)

**Authorization Note:** Current application has no authentication/authorization. Admin and manager features should be accessible but note that existing design does not restrict access.

#### 8. **Project Structure**
```
Assignment 2/
├── Controllers/
│   ├── HomeController.cs
│   ├── SearchController.cs
│   └── [No Admin/Manager controllers yet]
├── LibraryManagement/
│   ├── Item.cs (abstract)
│   ├── Book.cs, Music.cs, Toy.cs
│   ├── Borrower.cs, BorrowRecord.cs
│   ├── AdminService.cs
│   ├── ManagerService.cs
│   ├── SearchService.cs
│   ├── DataService.cs (static, handles JSON I/O)
│   └── ItemJsonConverter.cs
├── Views/
│   ├── Home/
│   ├── Search/
│   └── Shared/ (_Layout.cshtml)
├── Models/
│   └── ErrorViewModel.cs
├── Properties/
│   └── launchSettings.json
├── wwwroot/ (CSS, JS, libraries)
├── Program.cs (WebAppStartup entry point)
├── LibraryManagementConsole.cs (console entry point)
├── appsettings.json, appsettings.Development.json
└── PROGRAMMERS_LOG.md, README.md
```

#### 9. **Package Dependencies**
- **Current Packages:** Only .NET 10 SDK (no NuGet packages added)
- **Available:** System.Text.Json (built-in)
- **For PDF Export:** Will need to evaluate lightweight options (e.g., QuestPDF, SelectPdf) or HTML-to-PDF solution

#### 10. **Existing Data Format (JSON Example)**
```json
[
  {
	"$type": "Book",
	"name": "Example Book",
	"description": "A sample book",
	"libraryCode": "BOOK001",
	"status": "Available",
	"author": "Author Name",
	"genre": "Fiction"
  },
  {
	"$type": "Music",
	"name": "Album Title",
	"description": "A sample album",
	"libraryCode": "MUSIC001",
	"status": "Borrowed",
	"artist": "Artist Name",
	"year": 2020
  },
  {
	"$type": "Toy",
	"name": "Toy Name",
	"description": "A sample toy",
	"libraryCode": "TOY001",
	"status": "Available",
	"type": "Building Blocks",
	"ageGroup": "3-8"
  }
]
```

---

## Design Proposal for Stage 2+

### Feature 1: CSV Catalogue Importer

**Proposed CSV Format:**
```
Type,Name,Description,LibraryCode,Status,Author,Genre,Artist,Year,ToyType,AgeGroup
Book,The Hobbit,Fantasy classic,BOOK001,Available,J.R.R. Tolkien,Fantasy,,,
Music,Abbey Road,Famous album,MUSIC001,Available,,,,The Beatles,1969,
Toy,Lego Set,Building toy,TOY001,Available,,,,,Building Blocks,4-12
```

**Import Service:**
- Parse CSV rows (skip header, handle blank rows)
- Validate required fields per type (Type, Name, LibraryCode, Status)
- Check for duplicate library codes
- Create appropriate Item subclass via AdminService
- Accumulate success/failure logs
- Persist via DataService.SaveAllData() on completion

**Validation Rules:**
- Status must be one of: Available, Borrowed, Damaged, Destroy
- LibraryCode must be unique
- LibraryCode, Name are required; Description can be empty
- Type-specific fields must match expected types (Year as integer, etc.)

### Feature 2: Manager Analytics Dashboard

**Proposed Metrics:**
1. **Catalogue Statistics:**
   - Total items by type (Books, Music, Toys)
   - Items by status (Available, Borrowed, Damaged, Destroy)
   - Items per category (if applicable)

2. **Borrowing Activity:**
   - Total items currently borrowed
   - Total borrowers
   - Items overdue
   - Total fines owed

3. **Detailed Reports (per type):**
   - Books: Count by Author, Genre
   - Music: Count by Artist
   - Toys: Count by Age Group

**Data Source:** Existing ManagerService calculations + LINQ queries on static collections.

### Feature 3: CSV and PDF Exports

**CSV Export:**
- Generate same format as import CSV for consistency
- Escape commas, quotes, newlines correctly
- Include timestamp/report date

**PDF Export:**
- Minimal library (evaluate QuestPDF, iText, or HTML-to-PDF)
- Include report title, generation date, summary statistics
- Formatted tables for detailed data
- Professional appearance

---

## Integration Points and Risks

### Safeguards Implemented (Proposal)
1. **Duplicate Detection:** Existing `AdminService.LibraryCodeExists()` prevents duplicates
2. **Validation:** Reuse Item and AdminService validation logic
3. **Atomic Persistence:** Import validates entire batch before calling `DataService.SaveAllData()`
4. **Input Sanitization:** Validate CSV content before parsing; file type check (not extension alone)
5. **No Breaking Changes:** New controllers/services added; existing code untouched

### Known Limitations
1. **No Authentication:** Existing app doesn't enforce admin/manager roles; new features will be accessible to all
   - Workaround: Document that administrator should host in controlled environment or add auth later
2. **Polymorphic Serialization:** Custom ItemJsonConverter must be maintained for persistence
3. **In-Memory Data:** Static collections mean data loss if app crashes; persist frequently
4. **No Transactions:** CSV import not atomic at file level; partial failures possible if save fails

### Testing Strategy
1. Valid CSV with all item types
2. Empty/missing columns
3. Invalid status values
4. Duplicate library codes
5. Malformed CSV (unmatched quotes, embedded newlines)
6. Large files (establish reasonable limit)
7. Existing data preservation after import
8. Analytics calculations with various data scenarios
9. CSV export correctness (escaping, format)
10. PDF export rendering

---

## Proposed Implementation Plan

### Stage 2: Design & Planning
- Finalize CSV column mapping
- Design analytics data structure and calculations
- Evaluate PDF library options

### Stage 3: CSV Import Service
- `CatalogueImportService` class
- CSV parsing using built-in System.Text.Json or CsvHelper (lightweight)
- Validation and error logging

### Stage 4: Admin Import Web Interface
- `ImportController` or extend existing admin namespace
- File upload form
- Import results display
- Error/success messages

### Stage 5: Import Testing & Validation
- End-to-end import workflow
- Edge cases and error handling
- Data persistence verification

### Stage 6: Manager Analytics Dashboard
- `AnalyticsController`
- Dashboard view with charts/tables
- Delegate calculations to ManagerService or new `ReportingService`

### Stage 7: CSV/PDF Exports
- Export service layer
- Export endpoints in AnalyticsController
- File download mechanism

### Stage 8: Final Testing & Documentation
- Comprehensive testing
- Update README and log
- Commit and push

---

## Status Summary

| Item | Status | Notes |
|------|--------|-------|
| **Project Build** | ✅ Success | No errors, clean build |
| **Current Feature Inspection** | ✅ Complete | All components understood |
| **Migration Risks** | ✅ Assessed | No existing admin/manager web UI; safe to add |
| **Data Persistence** | ✅ Verified | JSON-based, reusable via DataService |
| **Design Ready** | ⏳ Pending | Stage 2 to finalize details |

---

### Key ManagerService Findings

Inspected full `ManagerService.cs` which provides **reusable statistics methods** already designed to support analytics:

**Available Statistics Classes:**
- `BorrowingStatistics` – currently borrowed count, total borrows, top 5 most borrowed items
- `ItemStatusStatistics` – counts by status (Available, Borrowed, Damaged, Destroy), total items, percentage breakdown
- `FineStatistics` – total fines collected, late return count, average fine, overdue items, current overdue amount
- `LibraryStatistics` – comprehensive wrapper containing all above stats

**Key Methods (Non-Console):**
- `GetBorrowingStatistics()` → returns structured data (not Console output)
- `GetItemStatusStatistics()` → returns structured data
- `GetFineStatistics()` → returns structured data
- `GetComprehensiveStatistics()` → returns all statistics in one call

**Insight:** ManagerService already has perfect separation! The `Get*Statistics()` methods return domain objects that can be passed directly to web views; the `Display*Statistics()` methods are console-specific and can be ignored.

### AdminService Detailed Analysis

- **Duplicate Detection:** `LibraryCodeExists(string libraryCode)` – perfect foundation for importer validation
- **Create Methods:** `CreateBook()`, `CreateMusic()`, `CreateToy()` – returns bool, validates status, catches ArgumentException
- **Update Methods:** `UpdateItem()` by property name
- **Search Methods:** `FindByLibraryCode()`, `SearchByName()`
- **All methods write to Console** – for importer, we'll need to either suppress output or create a parallel non-console API

---

## Stage 2 Detailed Design

### CSV Importer Specification

**CSV Format and Column Mapping:**

Proposed CSV structure with type-aware optional columns:

```csv
Type,Name,Description,LibraryCode,Status,Author,Genre,Artist,Year,ToyType,AgeGroup
Book,"The Hobbit","Fantasy adventure in Middle-earth",BOOK001,Available,"J.R.R. Tolkien","Fantasy",,,,
Music,"Abbey Road","The Beatles iconic album",MUSIC001,Available,,,"The Beatles",1969,,
Toy,"LEGO Classic","Building blocks set",TOY001,Available,,,,,Building Blocks,4-12
```

**Validation Rules (Per Row):**
1. **Required fields (all types):**
   - `Type` ∈ {Book, Music, Toy}
   - `Name` – non-empty, max 255 chars
   - `LibraryCode` – non-empty, unique (checked against Item.AllItems)
   - `Status` ∈ {Available, Borrowed, Damaged, Destroy}
   - `Description` – optional

2. **Type-Specific Validations:**
   - **Book:** Requires `Author` and `Genre` (non-empty)
   - **Music:** Requires `Artist` (non-empty) and `Year` (valid integer, e.g., 1900–2100)
   - **Toy:** Requires `ToyType` (called "Type" in model) and `AgeGroup` (non-empty)

3. **Error Handling:**
   - Skip blank rows silently
   - Log invalid rows with line number and specific field error
   - Do NOT insert partial records
   - Accumulate all validation errors before displaying summary

**Import Result Structure:**
```
ImportResult
├── SuccessCount: int
├── FailureCount: int
├── ImportedItems: List<Item>
└── FailedRows: List<ImportError>
    ├── RowNumber: int
    ├── RawContent: string
    └── ErrorMessage: string
```

### Analytics Dashboard Specification

**Dashboard Metrics (Using ManagerService):**

1. **Catalogue Overview:**
   ```
   Total Items:        125
   Books:              45   (36%)
   Music:              50   (40%)
   Toys:               30   (24%)
   ```

2. **Item Status Distribution:**
   ```
   Available:          95   (76%) [Green bar]
   Borrowed:           20   (16%) [Yellow bar]
   Damaged:            8    (6%)  [Red bar]
   Marked for Destroy: 2    (2%)  [Dark Red bar]
   ```

3. **Borrowing Activity:**
   ```
   Currently Borrowed:  20 items
   Total Transactions:  150 borrows
   Most Borrowed Item:  [Top 5 list]
   ```

4. **Fine Statistics:**
   ```
   Total Fines Collected:   $245.50
   Overdue Items:           5
   Current Overdue Amount:  $8.00
   Late Return Rate:        12.5%
   ```

5. **Detailed Analytics (By Type):**
   - Books by Genre (count)
   - Music by Artist (count)
   - Toys by Age Group (count)

**Dashboard Access:**
- New route: `/Analytics/Dashboard` or `/Manager/Analytics`
- New controller: `AnalyticsController` (or extend existing pattern)
- New view: `Views/Analytics/Dashboard.cshtml`

### CSV and PDF Export Specification

**CSV Export Schema:**
```
Report Type,Report Date,Total Items,Available,Borrowed,Damaged,Destroy,...
Manager Analytics,2026-01-15,125,95,20,8,2,...
```

Export logic:
- Generate CSV using built-in StringBuilder or lightweight CsvHelper (if justified)
- Escape quotes, commas, newlines per RFC 4180
- Include report metadata (generation date, exported by, etc.)
- Support filtered exports (e.g., only books, only overdue items)

**PDF Export Approach:**
Options evaluated:
1. **QuestPDF** – lightweight, free, .NET 10 compatible, no external dependencies
2. **SelectPdf** – paid/trial
3. **iText** – complex, licensing
4. **HTML-to-PDF (NReco)** – simple but requires HTML generation

**Decision:** Defer final choice to implementation stage; evaluate if QuestPDF adds acceptable overhead.

---

## Integration Architecture

### No Breaking Changes
- Existing Item, AdminService, DataService unchanged
- New AnalyticsController, ImportController separate
- New ServiceLayer:
  - `CatalogueImportService` – handles CSV processing
  - `ReportExportService` – handles CSV/PDF generation
  - Dashboard uses existing `ManagerService` for calculations

### Data Flow
```
CSV File → [ImportService validates] → [AdminService.Create*] → [DataService.SaveAllData()]
           ↓
        Item.AllItems (updated)
           ↓
        [ManagerService.Get*Statistics] ← [AnalyticsController queries]
           ↓
        Dashboard View displays metrics
           ↓
        ExportService transforms to CSV/PDF
           ↓
        File download
```

### Authorization Consideration
**Current State:** No web authorization configured.  
**Approach:** Create `[Authorize]` attributes on controller actions as placeholders; document that admin/manager authentication should be configured separately or controllers placed behind reverse proxy/auth layer.

---

## Risk Assessment and Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|-----------|
| CSV parsing errors corrupt data | Medium | High | Validate entire batch before persist; use try-catch; preserve existing items |
| Duplicate library codes allow duplicates | Medium | High | Check `AdminService.LibraryCodeExists()` before insert; reject row if duplicate |
| Large CSV files cause memory issue | Low | Medium | Implement file size check (e.g., max 10 MB); stream if needed later |
| Type-specific field validation fails | Medium | Medium | Per-type validation logic; clear error messages |
| PDF export bloats project size | Low | Low | Use lightweight library; defer if not critical |
| Console output pollutes import results | Medium | Low | Create non-console overload of AdminService or suppress output |
| Existing JSON data loss on save failure | Low | Critical | Backup before save; verify write success; atomic operations where possible |

---

## Known Limitations (Design Phase)

1. **No Authentication/Authorization:** Existing app has no role enforcement; admin/manager features will be accessible to all users until auth is added project-wide.
2. **In-Memory Data:** Static collections mean no data persistence during app crash; reliant on DataService.SaveAllData() calls.
3. **CSV Column Order Matters:** Importer expects exact column headers; will fail if headers are reordered (can be improved later with dictionary-based parsing).
4. **No Transaction Rollback:** If import partially succeeds and then save fails, some items remain in memory but not persisted.

---

## Summary and Readiness

**Stage 2 Analysis Complete:**
- ✅ CSV column mapping finalized
- ✅ Analytics data structures confirmed (reuse ManagerService outputs)
- ✅ Export approaches evaluated
- ✅ Integration points identified
- ✅ Risks documented
- ✅ No breaking changes required

**Recommendation for Stage 3:**
Proceed to implement **CatalogueImportService** with full validation and error reporting.

---

*End of Stage 2 Planning*
