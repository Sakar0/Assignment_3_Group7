# Programmer's Log - Library Management System (Assignment 3)

**Date:** 2026
**Project:** Library Management System  
**Language:** C# (.NET 10)
**Team:** Aryan, Sakar, Samir, Rohan

## Project Overview

Built a library management system where students can manage books, music, and toys. The system includes:
- Admin panel to add/edit/delete items
- Reception desk to manage borrowers and check-outs
- Manager dashboard to view statistics
- Public web search page for anyone to search items
- Data saving to files so the library's data doesn't get lost

## What Was Built

### 1. Item System (Base Item class + Book, Music, Toy classes)
- Purpose: Store information about library items
- Item Base Class: Common properties (Name, Description, Library Code, Status)
- Status Values: Available, Borrowed, Damaged, Destroy
- Book Class: Adds Author and Genre
- Music Class: Adds Artist and Year
- Toy Class: Adds Type and Age Group
- All Items: Stored in a shared list (Item.AllItems)

### 2. Admin Service
- Add new items (books, music, toys)
- Search for items by code or name
- Edit item information
- Delete items
- Display all items

### 3. Borrower System
- Store borrower information (Name, Phone, Email)
- Track who borrowed what
- Track when items were borrowed and returned
- Calculate overdue fines ($1 per day after 14 days)

### 4. Reception Desk Service
- Manage borrower records (add, edit, delete borrowers)
- Checkout items (mark as borrowed)
- Return items (mark as available again)
- View overdue items and fines

### 5. Manager Dashboard
- Shows borrowing statistics (how many items out)
- Shows item status (available vs borrowed vs damaged)
- Shows fines collected
- Comprehensive reports combining all statistics

### 6. Public Web Search
- Anyone can search for items by name, code, or type
- View item details
- Mobile-friendly design
- Read-only (cannot borrow or edit)

### 7. Data Persistence (Save/Load)
- Saves all data to JSON files (so data persists when program closes)
- When program starts, it tries to load saved data
- If no saved data, creates sample items
- Users can manually save/load data from the menu

### 8. Console Menu
- Role-based menu (Admin, Reception, Manager)
- Save Data option
- Load Data option
- Exit button

### 9. Unified Web Program
- Keeps data in memory (same data for console and web)
- Web app loads data on startup
- Search portal always has items to display

## Main Features

**Admin Functions:**
- Create items (books, music, toys)
- Search items by library code or name
- Update item information
- Delete items
- View all items

**Reception Functions:**
- Add new borrowers
- Edit borrower details
- Check out items to borrowers
- Return items from borrowers
- View overdue items and calculate fines

**Manager Functions:**
- See how many items are out
- See item status breakdown
- Calculate fines
- View comprehensive reports

**Public Search:**
- Search items online
- Mobile-friendly interface
- Read-only (safe for public)

**Data Saving:**
- Automatic load on startup
- Manual save/load from menu
- JSON files (easy to backup and inspect)

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

## Files Created

### Core Classes:
- `Item.cs` - Base class for all items
- `Book.cs`, `Music.cs`, `Toy.cs` - Item types
- `Borrower.cs` - Borrower information
- `BorrowRecord.cs` - Tracks who borrowed what

### Services:
- `AdminService.cs` - Item management
- `ReceptionService.cs` - Borrower and checkout management
- `ManagerService.ps` - Statistics and reporting
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

## Data Files

When program saves data, it creates these files:
- `data/items.json` - All library items
- `data/borrowers.json` - All borrower information
- `data/borrow_records.json` - All checkout/return history

You can open these files with any text editor to view or manually edit the data.

## Build Status

Compiles Successfully - No errors or warnings

## Notes for Submission

- Only one console entry point: `LibraryManagementConsole.cs`
- Consistent namespaces: All use `Assignment2`
- Web app auto-loads data on startup
- Public search is read-only (safe for students)
- Data persists using JSON files
- All SOLID principles applied
- Code is clean and well-commented

## Team Contributions

See individual programmer logs for detailed work:
- **Aryan:** Self-service library kiosk and secure read-only REST API
- **Sakar:** Multi-branch inventory and inter-branch item transfer features
- **Samir:** Reservations, waitlist, and notification system
- **Rohan:** Analytics, import/export, and bulk catalogue importer

Individual logs available in:
- `ProgrammersLog-Aryan.md`
- `ProgrammersLog-Sakar.md`
- `ProgrammersLog-Samir.md`
- `ProgrammersLog-Rohan.md`

## Future Improvements

If this project is expanded:
- Add database instead of JSON files
- Add user authentication (student ID login)
- Email notifications for overdue items
- Mobile app for borrowers
