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

## Next Steps

**Stage 2:** Detail the CSV importer design and analytics calculations, finalize column mappings and report structure.

---

*End of Stage 1 Inspection*
