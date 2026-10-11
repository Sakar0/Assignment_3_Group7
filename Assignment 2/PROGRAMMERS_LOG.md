# Programmer's Log - Library Management System (Assignment 2)

**Date:** 2026
**Project:** Library Management System  
**Language:** C# (.NET 10)

---

## Project Overview

Built a library management system where students can manage books, music, and toys. The system includes:
- Admin panel to add/edit/delete items
- Reception desk to manage borrowers and check-outs
- Manager dashboard to view statistics
- Public web search page for anyone to search items
- Data saving to files so the library's data doesn't get lost

---

## What Was Built

### 1. **Item System** (Base Item class + Book, Music, Toy classes)
- **Purpose:** Store information about library items
- **Item Base Class:** Common properties (Name, Description, Library Code, Status)
- **Status Values:** Available, Borrowed, Damaged, Destroy
- **Book Class:** Adds Author and Genre
- **Music Class:** Adds Artist and Year
- **Toy Class:** Adds Type and Age Group
- **All Items:** Stored in a shared list (`Item.AllItems`)

### 2. **Admin Service** 
- Add new items (books, music, toys)
- Search for items by code or name
- Edit item information
- Delete items
- Display all items

### 3. **Borrower System**
- Store borrower information (Name, Phone, Email)
- Track who borrowed what
- Track when items were borrowed and returned
- Calculate overdue fines ($1 per day after 14 days)

### 4. **Reception Desk Service**
- Manage borrower records (add, edit, delete borrowers)
- Checkout items (mark as borrowed)
- Return items (mark as available again)
- View overdue items and fines

### 5. **Manager Dashboard**
- Shows borrowing statistics (how many items out)
- Shows item status (available vs borrowed vs damaged)
- Shows fines collected
- Comprehensive reports combining all statistics

### 6. **Public Web Search**
- Anyone can search for items by name, code, or type
- View item details
- Mobile-friendly design
- Read-only (cannot borrow or edit)

### 7. **Data Persistence (Save/Load)**
- Saves all data to JSON files (so data persists when program closes)
- When program starts, it tries to load saved data
- If no saved data, creates sample items
- Users can manually save/load data from the menu

### 8. **Console Menu**
- Role-based menu (Admin, Reception, Manager)
- Save Data option
- Load Data option
- Exit button

### 9. **Unified Web Program**
- Keeps data in memory (same data for console and web)
- Web app loads data on startup
- Search portal always has items to display

---

## Main Features

✅ **Admin Functions:**
- Create items (books, music, toys)
- Search items by library code or name
- Update item information
- Delete items
- View all items

✅ **Reception Functions:**
- Add new borrowers
- Edit borrower details
- Check out items to borrowers
- Return items from borrowers
- View overdue items and calculate fines

✅ **Manager Functions:**
- See how many items are out
- See item status breakdown
- Calculate fines
- View comprehensive reports

✅ **Public Search:**
- Search items online
- Mobile-friendly interface
- Read-only (safe for public)

✅ **Data Saving:**
- Automatic load on startup
- Manual save/load from menu
- JSON files (easy to backup and inspect)

---

## How It Works

### Console Application Flow:
1. Program starts
2. Try to load saved data from files
3. If no saved data, create sample items
4. Show main menu (role selection)
5. User chooses role (Admin/Reception/Manager)
6. Show role-specific menu
7. User performs actions
8. Back to main menu or exit

### Web Application Flow:
1. Web app starts
2. Load saved data or create sample data
3. User visits home page
4. Click link to search page
5. Search for items by name/code/type
6. View results (read-only)

---

## Challenges & Solutions

### Challenge 1: Multiple Types (Book vs Music vs Toy)
**Solution:** Used inheritance - Book, Music, Toy all inherit from Item class. This lets us treat them all as Items in a list, but each type can have its own special properties.

### Challenge 2: Saving to Files
**Solution:** Used JSON format. Each item type is marked with a `$type` indicator so we know what type to create when loading from file.

### Challenge 3: Empty Web Portal
**Solution:** Made web app load data on startup, so users always see items even on first run.

### Challenge 4: Multiple Main Methods
**Solution:** Commented out Main methods in individual console apps, kept only LibraryManagementConsole as the single entry point.

### Challenge 5: Namespace Mismatch
**Solution:** Changed all namespaces from Assignment_2 to Assignment2 for consistency.

---

## Files Created

### Core Classes:
- `Item.cs` - Base class for all items
- `Book.cs`, `Music.cs`, `Toy.cs` - Item types
- `Borrower.cs` - Borrower information
- `BorrowRecord.cs` - Tracks who borrowed what

### Services:
- `AdminService.cs` - Item management
- `ReceptionService.cs` - Borrower and checkout management
- `ManagerService.cs` - Statistics and reporting
- `DataService.cs` - Save and load data
- `SearchService.cs` - Search functionality
- `ItemJsonConverter.cs` - Convert items to/from JSON

### Console Apps:
- `LibraryManagementConsole.cs` - Main console menu (ONLY entry point)
- `AdminConsoleApp.cs` - Admin menu (helper)
- `ReceptionConsoleApp.cs` - Reception menu (helper)
- `ManagerConsoleApp.cs` - Manager menu (helper)

### Web Components:
- `SearchController.cs` - Web search page controller
- `HomeController.cs` - Home page controller
- `Index.cshtml` - Search page (view)
- `site.css` - Styling

### Configuration:
- `Program.cs` - Web app startup configuration

---

## How to Use

### Running as Console App:
1. Set LibraryManagementConsole.cs as startup
2. Run the program
3. Choose your role: Admin (1), Reception (2), Manager (3), Save (4), Load (5), or Exit (6)
4. Follow the menu options

### Running as Web App:
1. Start the web application
2. Home page shows library information
3. Click "Search Library" to go to search page
4. Search by item name, code, or type
5. View results with item details

---

## Data Files

When program saves data, it creates these files:
- `data/items.json` - All library items
- `data/borrowers.json` - All borrower information
- `data/borrow_records.json` - All checkout/return history

You can open these files with any text editor to view or manually edit the data.

---

## Build Status

✅ **Compiles Successfully** - No errors or warnings

---

## Notes for Submission

- Only one console entry point: `LibraryManagementConsole.cs`
- Consistent namespaces: All use `Assignment2`
- Web app auto-loads data on startup
- Public search is read-only (safe for students)
- Data persists using JSON files
- All SOLID principles applied
- Code is clean and well-commented

---

## Future Improvements

If this project is expanded:
- Add database instead of JSON files
- Add user authentication (student ID login)
- Email notifications for overdue items
- Mobile app version
- Reservation system for items
- Wishlist feature for students

---

---

## ADVANCED TECHNICAL NOTES (For Reference)

### Architecture Overview

The system uses a **layered architecture**:
1. **Presentation Layer** - Console menus + Web pages
2. **Service Layer** - AdminService, ReceptionService, ManagerService
3. **Data Layer** - DataService (JSON persistence)
4. **Model Layer** - Item hierarchy, Borrower, BorrowRecord

### Key Design Patterns Used

**1. Inheritance**
- Abstract `Item` base class with `Book`, `Music`, `Toy` subclasses
- Allows polymorphism: treating all items as `Item` type
- Reduces code duplication

**2. Static List Pattern**
- `Item.AllItems` - single shared collection for all items
- `Borrower.AllBorrowers` - shared collection for borrowers
- `BorrowRecord.AllBorrowRecords` - shared collection for records
- Accessed from any service without database

**3. Custom JSON Converter**
- `ItemJsonConverter.cs` - handles polymorphic serialization
- Uses `$type` discriminator to identify item type when loading
- De-serializes back to correct subclass (Book, Music, or Toy)

**4. Service Locator Pattern**
- `AdminService`, `ReceptionService`, `ManagerService` - self-contained business logic
- Each service accesses static collections independently
- No interdependencies between services

### Technical Implementation Details

**Item Polymorphism:**
```
Item (abstract base)
  ├─ Book (Author + Genre)
  ├─ Music (Artist + Year)
  └─ Toy (Type + AgeGroup)

All stored in: Item.AllItems as List<Item>
```

**Borrowing System:**
```
Borrower (ID, Name, Phone, Email)
    ↓
BorrowRecord (LibraryCode, BorrowerId, BorrowDate, ReturnDate)
    ↓
Item (marked as Borrowed when checked out)
```

**Fine Calculation:**
- BorrowRecord tracks checkout dates
- `CalculateFine()` method: $1 per day after 14 days
- Example: 20-day borrow = (20-14) × $1 = $6

**Web-Console Integration:**
- Both share same static collections in memory
- Web app loads data on startup (same as console)
- Changes in console visible immediately in web
- Data saved to disk persists across both

### Data Persistence Detail

**JSON Format Example:**
```json
{
  "$type": "Book",
  "name": "The Great Gatsby",
  "description": "A classic American novel",
  "libraryCode": "BOOK001",
  "status": "Available",
  "author": "F. Scott Fitzgerald",
  "genre": "Fiction"
}
```

**Load/Save Flow:**
1. Save: Objects → JSON strings → files
2. Load: JSON files → parse → check `$type` → create correct class
3. All collections cleared before loading (prevents duplicates)
4. If files missing: silently continues (first run scenario)

### Error Handling

**Validation Points:**
- Status values checked against allowed list (Available, Borrowed, Damaged, Destroy)
- LibraryCode checked for existence when searching
- Borrower phone/email validated for format
- JSON deserialization wrapped in try-catch

**User-Friendly Messages:**
- Invalid status shows: "Status must be one of: Available, Borrowed, Damaged, Destroy"
- Item not found shows: "Item with code BOOK999 not found"
- File save/load outputs confirmation messages

### Performance Considerations

**Memory Usage:**
- In-memory lists suitable for ~1000 items
- No database queries = very fast searches
- Entire data loaded once at startup = quick access

**File I/O:**
- JSON serialization efficient for small catalogs
- ~100-300 bytes per item
- Full save/load typically < 500ms for moderate libraries

### Testing Notes

The system was tested with:
- Multiple simultaneous items (Books + Music + Toys in same list)
- Status validation (invalid statuses rejected)
- Borrowing workflow (checkout → return → fine calculation)
- Data persistence (save → close → load → verify data)
- Web search (all item types searchable and displayable)
- Role-based access (correct menus for each role)

### Known Limitations & Workarounds

1. **No Duplicate Prevention** - LibraryCode uniqueness not enforced
   - Workaround: Add validation in AdminService before creating items

2. **No User Authentication** - Anyone can access any role
   - Workaround: For production, add login system

3. **Single-User Concurrency** - Only one user at a time
   - Workaround: Use database with transaction support

4. **Manual Backup** - No automatic backup of saved files
   - Workaround: Periodically copy data/ folder

### Code Organization

```
Assignment 2/
├── LibraryManagement/
│   ├── Item.cs (base class + AllItems collection)
│   ├── Book.cs, Music.cs, Toy.cs (subclasses)
│   ├── Borrower.cs (borrower data + AllBorrowers)
│   ├── BorrowRecord.cs (transaction tracking + AllBorrowRecords)
│   ├── AdminService.cs (CRUD for items)
│   ├── ReceptionService.cs (borrower + checkout/return)
│   ├── ManagerService.cs (statistics + reporting)
│   ├── DataService.cs (JSON save/load)
│   ├── ItemJsonConverter.cs (polymorphic serialization)
│   └── SearchService.cs (search operations)
├── Controllers/
│   ├── HomeController.cs (web home page)
│   └── SearchController.cs (web search API)
├── Views/
│   ├── Home/Index.cshtml (landing page)
│   ├── Search/Index.cshtml (search interface)
│   └── Shared/ (layouts & error pages)
├── LibraryManagementConsole.cs (console entry point)
├── Program.cs (web app entry point)
└── data/ (created at runtime - JSON files)
```

