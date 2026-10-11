# Library Management System

## What This Project Does

This is a library management system where you can:
- Manage books, music, and toys in a library
- Add, edit, and delete items
- Let borrowers check out and return items
- Track who borrowed what
- Calculate overdue fines
- Search for items on a public website
- Save all data to files

The system has two parts:
1. A console menu for staff (Admin, Reception, Manager)
2. A website where students can search for items

---

## How to Run

**Console Application (Default):**
Just run the project normally. You will see the console menu with options:
- Admin (1) - Add, edit, delete items
- Reception (2) - Manage borrowers and check out items
- Manager (3) - View statistics
- Save Data (4) - Save to JSON files
- Load Data (5) - Load from JSON files
- Exit (6) - Close the program

**Web Application (Search Portal):**
Run the project with the `web` argument: `dotnet run -- web`
Then go to the home page and click "Search Library" to search for items.

---

## Main Features

Admin can:
- Create books, music, and toys
- Search items by code or name
- Edit item details
- Delete items

Reception can:
- Manage borrowers
- Check out items
- Return items
- Calculate overdue fines

Manager can:
- View borrowing statistics
- See item status
- View fine reports

Public Search:
- Search items on website
- View item details
- Read-only (cannot borrow or edit)

---

## Data Files

When you save data, it creates these files:
- data/items.json - All library items
- data/borrowers.json - All borrower information
- data/borrow_records.json - All checkout/return history

---

## Prompts Used

1. Create an abstract Item class with Book, Music, and Toy types
2. Create AdminService to add, search, edit, and delete items
3. Create Borrower and BorrowRecord classes for tracking loans
4. Create ReceptionService to manage borrowers and check out items
5. Create ManagerService to show statistics and reports
6. Add public web search page for items
7. Save and load all data using JSON files
8. Clean up code and prepare for submission

---

## Technologies Used

- C# (.NET 10)
- ASP.NET Core
- JSON files for data storage
- Visual Studio 2026

---

## Files in Project

LibraryManagement/
- Item.cs (base class)
- Book.cs, Music.cs, Toy.cs (item types)
- Borrower.cs (borrower information)
- BorrowRecord.cs (track loans)
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
