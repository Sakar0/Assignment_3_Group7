# Library Management System - Assignment 3

## Overview

A comprehensive library management system for managing books, music, and toys. The application provides staff interfaces for administrative tasks (Admin, Reception, Manager roles) and a public web search portal for browsing the library catalogue. All data is persisted using JSON files.

## Project Type

- Language: C# (.NET 10)
- Framework: ASP.NET Core MVC
- Architecture: Single-tier application with in-memory data models and JSON persistence
- Platforms: Console application and web application

## Key Features

### Admin Functions
- Create new items (Books, Music, Toys) with type-specific properties
- Search items by library code or name
- Edit existing item details
- Delete items from the catalogue
- View complete item inventory

### Reception Desk Functions
- Add and manage borrower records
- Record borrower contact information (Name, Phone, Email)
- Check out items to borrowers
- Return items and calculate overdue charges
- Track borrowing history and active loans
- View overdue items and associated fines
- Calculate fines at $1 per day after 14-day loan period

### Manager Dashboard Functions
- View borrowing statistics (count of items checked out)
- Monitor item status breakdown (Available, Borrowed, Damaged, Destroy)
- Track total fines collected
- Generate comprehensive reports
- Access to overall library performance metrics

### Public Search Portal
- Browse library catalogue by keyword
- Search items by name, library code, or type
- View detailed item information
- Mobile-friendly responsive interface
- Read-only access (no borrowing or editing capabilities)

### Data Management
- Automatic data loading on application startup
- Manual save/load functionality through console menu
- JSON-based file storage for durability
- Support for item types with polymorphic deserialization
- Persistent storage of borrowers and transaction history

## System Architecture

### Data Models

**Item Hierarchy (Polymorphic):**
- Item (abstract base class) - Name, Description, Library Code, Status
- Book - Extends Item with Author and Genre
- Music - Extends Item with Artist and Release Year
- Toy - Extends Item with Type and Age Group classification

**Supporting Models:**
- Borrower - Stores patron information and tracks borrowing activity
- BorrowRecord - Maintains checkout/return history and fine calculations
- LibraryBranch - Multi-branch support (for branch management features)
- LibraryTransfer - Inter-branch item transfer tracking

### Data Persistence

Data is stored in JSON files within the `data/` directory:
- `items.json` - Complete item catalogue with polymorphic type indicators
- `borrowers.json` - All registered borrowers and contact information
- `borrow_records.json` - Complete transaction history with fine calculations

## How to Run

### Console Application (Default)
Run the application with default settings:
```
dotnet run --project "Assignment 2/Assignment 2.csproj"
```

You will be presented with a menu system:
1. Admin (1) - Item management and catalogue administration
2. Reception (2) - Borrower and checkout management
3. Manager (3) - Statistics and reporting
4. Save Data (4) - Persist data to JSON files
5. Load Data (5) - Restore data from JSON files
6. Exit (6) - Close application

### Web Application (Search Portal)
Run the application in web mode:
```
dotnet run --project "Assignment 2/Assignment 2.csproj" -- web
```

Then navigate to the application in your web browser. The home page will display library information and a link to the search portal where you can query the catalogue.

## Services and Components

### Core Services
- **AdminService** - Handles all item CRUD operations
- **ReceptionService** - Manages borrowers and checkout/return transactions
- **ManagerService** - Generates statistics and reports
- **SearchService** - Provides catalogue search and filtering capabilities
- **DataService** - Handles JSON serialization and file persistence
- **ItemJsonConverter** - Manages polymorphic deserialization of item types

### Web Components
- **SearchController** - Exposes search functionality via web interface
- **HomeController** - Manages home page and navigation
- **Index.cshtml** - Search portal user interface
- **_Layout.cshtml** - Shared layout template
- **site.css** - Application styling

### Console Components
- **LibraryManagementConsole** - Primary console entry point
- **AdminConsoleApp** - Admin menu implementation
- **ReceptionConsoleApp** - Reception menu implementation
- **ManagerConsoleApp** - Manager menu implementation

## Item Status Values

- Available - Item is in stock and may be borrowed
- Borrowed - Item is currently checked out
- Damaged - Item requires repair or evaluation
- Destroy - Item has been decommissioned

## Loan Policies

- Standard loan period: 14 days
- Overdue charges: $1.00 per day after loan period expires
- Automatic calculation on item return
- Fine tracking per borrower record

## File Structure

```
Assignment 2/
├── LibraryManagement/
│   ├── Item.cs (base class) + Book.cs, Music.cs, Toy.cs
│   ├── Borrower.cs + BorrowRecord.cs
│   ├── LibraryBranch.cs + LibraryTransfer.cs
│   ├── AdminService.cs
│   ├── ReceptionService.cs
│   ├── ManagerService.cs
│   ├── SearchService.cs
│   ├── DataService.cs
│   ├── ItemJsonConverter.cs
│   └── CatalogueImportService.cs
├── Controllers/
│   ├── HomeController.cs
│   ├── SearchController.cs
│   └── ReservationController.cs
├── Views/
│   ├── Home/Index.cshtml
│   ├── Search/Index.cshtml
│   └── Shared/_Layout.cshtml
├── LibraryManagementConsole.cs (console entry point)
├── Program.cs (web app configuration)
├── Assignment 2.csproj
└── data/ (created at runtime)
```

## Dependencies

- System.Text.Json - For JSON serialization
- ASP.NET Core MVC - Web framework
- .NET Runtime 10.0

## Team Contributions

Individual developer work is documented in separate programmer logs:
- ProgrammersLog-Aryan.md - Kiosk and REST API implementation
- ProgrammersLog-Sakar.md - Multi-branch and transfer features
- ProgrammersLog-Samir.md - Reservations and notifications
- ProgrammersLog-Rohan.md - Analytics and import/export functionality

## Building and Testing

Build the solution:
```
dotnet build "Assignment 2.slnx"
```

Run all tests:
```
dotnet test "Assignment 2.slnx"
```

## Submission Notes

- Single console entry point: LibraryManagementConsole.cs
- Consistent code organization with clear separation of concerns
- No external database required - JSON files for storage
- Automatic data initialization on first run
- Web application auto-loads data at startup
- Public search interface is read-only and safe for student access
- All code follows SOLID principles
- Comprehensive error handling and validation

## Future Enhancements

Potential improvements for future versions:
- Migrate to SQL database (Entity Framework Core)
- Add user authentication system with student ID login
- Email notifications for overdue items
- Mobile application for borrowers
- Advanced reporting and analytics dashboard
- Item barcode scanning support
- Real-time inventory updates
- Integration with external library systems
- AdminService.cs (item management)
- ReceptionService.cs (borrow/return)
- ManagerService.cs (statistics)
- DataService.cs (save/load)
- ItemJsonConverter.cs (JSON handling)

Controllers/
- SearchController.cs (web search)
- HomeController.cs (home page)

LibraryManagementConsole.cs (main console menu)
Program.cs (web app startup)

---

## Build Status

Compiles successfully with no errors.
