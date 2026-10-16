# Library Management System – Horizontal Feature Expansion

**HIT339 Group Assignment 3 · Group 7 · Charles Darwin University**

ASP.NET Core MVC library system (.NET 10) expanded with multi-branch inventory, kiosk, read-only API, reservations, notifications, CSV import, and manager analytics.

**Repository:** https://github.com/Sakar0/Assignment_3_Group7  
**Baseline:** Sakar Khadka’s Assignment 2 codebase  

---

## Team

| Member | Student ID | Focus |
|--------|------------|--------|
| Sakar Khadka | s385095 | Coordinator, multi-branch inventory & transfers |
| Samir Rimal | s385096 | Reservations, waitlist & notifications |
| Aryan Rayamajhi | s385826 | Kiosk UI & public read-only API |
| Rohan Baniya | s386847 | CSV importer, analytics & export |

---

## Features

### Original (Assignment 2)
- Admin: create / edit / delete books, music, toys
- Reception: borrowers, checkout, return, fines
- Manager: statistics and reports
- Public search portal (name, library code, item type)

### New (Assignment 3 – horizontal expansion)
1. **Multi-branch inventory** – items tracked at Downtown (BR001), Campus (BR002), Riverside (BR003); inter-branch transfers
2. **Self-service kiosk** – touch-friendly route for checkout and account summary
3. **Public read-only API** – `GET /api/catalogue` (write requests return 405)
4. **Reservations & waitlist** – holds on borrowed/damaged items; notify next patron when available
5. **Notifications** – simulated email/SMS logged to a dashboard (borrowed, due soon, fines)
6. **CSV importer** – bulk import with validation and duplicate detection
7. **Analytics & export** – borrowing, fines, inventory reports; CSV/PDF export

---

## Requirements

- Git
- .NET 10 SDK (`dotnet --version` should show 10.x)
- Free TCP port (default **5108**)

Install SDK examples:
- **Ubuntu:** `sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0`
- **Windows:** `winget install Microsoft.DotNet.SDK.10`
- **macOS:** https://dotnet.microsoft.com/download/dotnet/10.0

---

## Quick start (developer machine)

```bash
git clone https://github.com/Sakar0/Assignment_3_Group7.git
cd Assignment_3_Group7
git checkout master

cd "Assignment 2"
dotnet run -- web
