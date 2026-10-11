using System;
using Assignment2.LibraryManagement;

namespace Assignment2
{
    /// <summary>
    /// Main entry point for the Library Management System Console Application
    /// Provides role-based access to Admin, Reception, and Manager functions
    /// </summary>
    public class LibraryManagementConsole
    {
        // Service instances
        private static AdminService adminService = new AdminService();
        private static ReceptionService receptionService = new ReceptionService();
        private static ManagerService managerService = new ManagerService();

        // Role enumeration
        private enum UserRole
        {
            Admin,
            Reception,
            Manager,
            SaveData,
            LoadData,
            Exit
        }

        // Main entry point - use this as the console app startup
        public static void Main(string[] args)
        {
            // Check if web app should run instead
            if (args.Length > 0 && args[0] == "web")
            {
                WebAppStartup.RunWebApp(args);
                return;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitializeSampleData();
            RunMainMenu();
        }

        /// <summary>
        /// Attempts to load saved data from files
        /// Returns true if data was loaded, false if no saved data exists
        /// </summary>
        private static bool AttemptLoadData()
        {
            if (DataService.HasSavedData())
            {
                Console.WriteLine("Attempting to load saved data...\n");
                return DataService.LoadAllData();
            }
            return false;
        }

        /// <summary>
        /// Main menu loop for role selection
        /// </summary>
        private static void RunMainMenu()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                DisplayWelcomeHeader();
                DisplayRoleMenu();

                string choice = Console.ReadLine();
                UserRole role = ParseRoleSelection(choice);

                switch (role)
                {
                    case UserRole.Admin:
                        RunAdminRole();
                        break;
                    case UserRole.Reception:
                        RunReceptionRole();
                        break;
                    case UserRole.Manager:
                        RunManagerRole();
                        break;
                    case UserRole.SaveData:
                        Console.Clear();
                        Console.WriteLine("\n=== SAVE DATA ===\n");
                        DataService.SaveAllData();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        break;
                    case UserRole.LoadData:
                        Console.Clear();
                        Console.WriteLine("\n=== LOAD DATA ===\n");
                        DataService.LoadAllData();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        break;
                    case UserRole.Exit:
                        running = false;
                        DisplayGoodbyeMessage();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.\n");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Admin role menu
        /// </summary>
        private static void RunAdminRole()
        {
            bool inAdminMenu = true;

            while (inAdminMenu)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              ADMIN - ITEM MANAGEMENT                      ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("1. Create Item");
                Console.WriteLine("2. View Item");
                Console.WriteLine("3. Search Items");
                Console.WriteLine("4. Update Item");
                Console.WriteLine("5. Delete Item");
                Console.WriteLine("6. Display All Items");
                Console.WriteLine("7. Return to Main Menu");
                Console.Write("\nSelect an option (1-7): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AdminCreateMenu();
                        break;
                    case "2":
                        AdminViewItem();
                        break;
                    case "3":
                        AdminSearchItems();
                        break;
                    case "4":
                        AdminUpdateItem();
                        break;
                    case "5":
                        AdminDeleteItem();
                        break;
                    case "6":
                        adminService.DisplayAllItems();
                        PressAnyKeyToContinue();
                        break;
                    case "7":
                        inAdminMenu = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.\n");
                        PressAnyKeyToContinue();
                        break;
                }
            }
        }

        /// <summary>
        /// Admin create item submenu
        /// </summary>
        private static void AdminCreateMenu()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              CREATE ITEM                                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("1. Create Book");
            Console.WriteLine("2. Create Music");
            Console.WriteLine("3. Create Toy");
            Console.WriteLine("4. Cancel");
            Console.Write("\nSelect an option (1-4): ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AdminCreateBook();
                    break;
                case "2":
                    AdminCreateMusic();
                    break;
                case "3":
                    AdminCreateToy();
                    break;
                case "4":
                    break;
                default:
                    Console.WriteLine("Invalid choice.\n");
                    break;
            }
        }

        private static void AdminCreateBook()
        {
            Console.WriteLine("\n--- Create New Book ---");
            Console.Write("Enter Book Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Description: ");
            string description = Console.ReadLine();

            Console.Write("Enter Library Code (e.g., BOOK001): ");
            string libraryCode = Console.ReadLine();

            adminService.DisplayAllowedStatuses();
            Console.Write("Enter Status: ");
            string status = Console.ReadLine();

            Console.Write("Enter Author: ");
            string author = Console.ReadLine();

            Console.Write("Enter Genre: ");
            string genre = Console.ReadLine();

            adminService.CreateBook(name, description, libraryCode, status, author, genre);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void AdminCreateMusic()
        {
            Console.WriteLine("\n--- Create New Music ---");
            Console.Write("Enter Music Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Description: ");
            string description = Console.ReadLine();

            Console.Write("Enter Library Code (e.g., MUSIC001): ");
            string libraryCode = Console.ReadLine();

            adminService.DisplayAllowedStatuses();
            Console.Write("Enter Status: ");
            string status = Console.ReadLine();

            Console.Write("Enter Artist: ");
            string artist = Console.ReadLine();

            Console.Write("Enter Year (e.g., 2020): ");
            if (!int.TryParse(Console.ReadLine(), out int year))
            {
                Console.WriteLine("Invalid year. Operation cancelled.\n");
                PressAnyKeyToContinue();
                return;
            }

            adminService.CreateMusic(name, description, libraryCode, status, artist, year);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void AdminCreateToy()
        {
            Console.WriteLine("\n--- Create New Toy ---");
            Console.Write("Enter Toy Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Description: ");
            string description = Console.ReadLine();

            Console.Write("Enter Library Code (e.g., TOY001): ");
            string libraryCode = Console.ReadLine();

            adminService.DisplayAllowedStatuses();
            Console.Write("Enter Status: ");
            string status = Console.ReadLine();

            Console.Write("Enter Toy Type: ");
            string type = Console.ReadLine();

            Console.Write("Enter Age Group (e.g., 3-8): ");
            string ageGroup = Console.ReadLine();

            adminService.CreateToy(name, description, libraryCode, status, type, ageGroup);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void AdminViewItem()
        {
            Console.WriteLine("\n--- View Item ---");
            Console.Write("Enter Library Code: ");
            string libraryCode = Console.ReadLine();

            var item = adminService.FindByLibraryCode(libraryCode);
            if (item != null)
            {
                Console.WriteLine($"Item: {item}\n");
            }
            PressAnyKeyToContinue();
        }

        private static void AdminSearchItems()
        {
            Console.WriteLine("\n--- Search Items ---");
            Console.Write("Enter name to search: ");
            string searchTerm = Console.ReadLine();

            var results = adminService.SearchByName(searchTerm);
            if (results.Count > 0)
            {
                Console.WriteLine($"\n--- Search Results for '{searchTerm}' ---");
                for (int i = 0; i < results.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {results[i]}");
                }
            }
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void AdminUpdateItem()
        {
            Console.WriteLine("\n--- Update Item ---");
            Console.Write("Enter Library Code of item to update: ");
            string libraryCode = Console.ReadLine();

            var item = adminService.FindByLibraryCode(libraryCode);
            if (item == null)
            {
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"\nCurrent item: {item}");
            Console.WriteLine("\nAvailable properties to update:");
            Console.WriteLine("- name");
            Console.WriteLine("- description");
            Console.WriteLine("- status");

            if (item is Book)
            {
                Console.WriteLine("- author");
                Console.WriteLine("- genre");
            }
            else if (item is Music)
            {
                Console.WriteLine("- artist");
                Console.WriteLine("- year");
            }
            else if (item is Toy)
            {
                Console.WriteLine("- type");
                Console.WriteLine("- agegroup");
            }

            Console.Write("\nEnter property name to update: ");
            string propertyName = Console.ReadLine();

            if (propertyName.ToLower() == "status")
            {
                adminService.DisplayAllowedStatuses();
            }

            Console.Write("Enter new value: ");
            string newValue = Console.ReadLine();

            adminService.UpdateItem(libraryCode, propertyName, newValue);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void AdminDeleteItem()
        {
            Console.WriteLine("\n--- Delete Item ---");
            Console.Write("Enter Library Code of item to delete: ");
            string libraryCode = Console.ReadLine();

            var item = adminService.FindByLibraryCode(libraryCode);
            if (item == null)
            {
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"\nItem to delete: {item}");
            Console.Write("Are you sure? (yes/no): ");
            string confirmation = Console.ReadLine();

            if (confirmation.ToLower() == "yes")
            {
                adminService.DeleteByLibraryCode(libraryCode);
            }
            else
            {
                Console.WriteLine("Delete cancelled.");
            }
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Reception role menu
        /// </summary>
        private static void RunReceptionRole()
        {
            bool inReceptionMenu = true;

            while (inReceptionMenu)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║            RECEPTION - BORROWER & CHECKOUT                ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("1. Manage Borrowers");
                Console.WriteLine("2. Borrow / Return Items");
                Console.WriteLine("3. View Reports");
                Console.WriteLine("4. Return to Main Menu");
                Console.Write("\nSelect an option (1-4): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        RunReceptionBorrowerMenu();
                        break;
                    case "2":
                        RunReceptionBorrowReturnMenu();
                        break;
                    case "3":
                        RunReceptionReportsMenu();
                        break;
                    case "4":
                        inReceptionMenu = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.\n");
                        PressAnyKeyToContinue();
                        break;
                }
            }
        }

        private static void RunReceptionBorrowerMenu()
        {
            bool inBorrowerMenu = true;

            while (inBorrowerMenu)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              BORROWER MANAGEMENT                         ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("1. Create Borrower");
                Console.WriteLine("2. View Borrower");
                Console.WriteLine("3. Search Borrowers");
                Console.WriteLine("4. Update Borrower");
                Console.WriteLine("5. Delete Borrower");
                Console.WriteLine("6. Display All Borrowers");
                Console.WriteLine("7. Back");
                Console.Write("\nSelect an option (1-7): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ReceptionCreateBorrower();
                        break;
                    case "2":
                        ReceptionViewBorrower();
                        break;
                    case "3":
                        ReceptionSearchBorrowers();
                        break;
                    case "4":
                        ReceptionUpdateBorrower();
                        break;
                    case "5":
                        ReceptionDeleteBorrower();
                        break;
                    case "6":
                        receptionService.DisplayAllBorrowers();
                        PressAnyKeyToContinue();
                        break;
                    case "7":
                        inBorrowerMenu = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.\n");
                        PressAnyKeyToContinue();
                        break;
                }
            }
        }

        private static void ReceptionCreateBorrower()
        {
            Console.WriteLine("\n--- Create New Borrower ---");
            Console.Write("Enter Borrower ID (e.g., BR001): ");
            string borrowerId = Console.ReadLine();

            Console.Write("Enter Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Phone: ");
            string phone = Console.ReadLine();

            Console.Write("Enter Email: ");
            string email = Console.ReadLine();

            receptionService.CreateBorrower(borrowerId, name, phone, email);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void ReceptionViewBorrower()
        {
            Console.Write("\nEnter Borrower ID: ");
            string borrowerId = Console.ReadLine();

            var borrower = receptionService.FindBorrowerById(borrowerId);
            if (borrower != null)
            {
                Console.WriteLine($"\nBorrower: {borrower}");
                var history = receptionService.GetBorrowerHistory(borrowerId);
                if (history.Count > 0)
                {
                    Console.WriteLine($"\nBorrow History ({history.Count} records):");
                    foreach (var record in history)
                    {
                        Console.WriteLine($"  - {record}");
                    }
                }
            }
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void ReceptionSearchBorrowers()
        {
            Console.Write("\nEnter name to search: ");
            string searchTerm = Console.ReadLine();

            var results = receptionService.SearchBorrowerByName(searchTerm);
            if (results.Count > 0)
            {
                Console.WriteLine($"\n--- Search Results for '{searchTerm}' ---");
                for (int i = 0; i < results.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {results[i]}");
                }
            }
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void ReceptionUpdateBorrower()
        {
            Console.WriteLine("\n--- Update Borrower ---");
            Console.Write("Enter Borrower ID: ");
            string borrowerId = Console.ReadLine();

            var borrower = receptionService.FindBorrowerById(borrowerId);
            if (borrower == null)
            {
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"\nCurrent borrower: {borrower}");
            Console.WriteLine("\nAvailable properties to update:");
            Console.WriteLine("- name");
            Console.WriteLine("- phone");
            Console.WriteLine("- email");

            Console.Write("\nEnter property name to update: ");
            string propertyName = Console.ReadLine();

            Console.Write("Enter new value: ");
            string newValue = Console.ReadLine();

            receptionService.UpdateBorrower(borrowerId, propertyName, newValue);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void ReceptionDeleteBorrower()
        {
            Console.WriteLine("\n--- Delete Borrower ---");
            Console.Write("Enter Borrower ID: ");
            string borrowerId = Console.ReadLine();

            var borrower = receptionService.FindBorrowerById(borrowerId);
            if (borrower == null)
            {
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"\nBorrower to delete: {borrower}");
            Console.Write("Are you sure? (yes/no): ");
            string confirmation = Console.ReadLine();

            if (confirmation.ToLower() == "yes")
            {
                receptionService.DeleteBorrower(borrowerId);
            }
            else
            {
                Console.WriteLine("Delete cancelled.");
            }
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void RunReceptionBorrowReturnMenu()
        {
            bool inBorrowMenu = true;

            while (inBorrowMenu)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              BORROW / RETURN ITEMS                       ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("1. Borrow an Item");
                Console.WriteLine("2. Return an Item");
                Console.WriteLine("3. Back");
                Console.Write("\nSelect an option (1-3): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ReceptionBorrowItem();
                        break;
                    case "2":
                        ReceptionReturnItem();
                        break;
                    case "3":
                        inBorrowMenu = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.\n");
                        PressAnyKeyToContinue();
                        break;
                }
            }
        }

        private static void ReceptionBorrowItem()
        {
            Console.WriteLine("\n--- Borrow an Item ---");
            Console.Write("Enter Library Code of item to borrow: ");
            string libraryCode = Console.ReadLine();

            var item = Item.AllItems.Find(i => i.LibraryCode == libraryCode);
            if (item == null)
            {
                Console.WriteLine($"Error: Item with LibraryCode '{libraryCode}' not found.\n");
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"Item: {item}");
            Console.Write("Enter Borrower ID: ");
            string borrowerId = Console.ReadLine();

            receptionService.BorrowItem(libraryCode, borrowerId);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void ReceptionReturnItem()
        {
            Console.WriteLine("\n--- Return an Item ---");
            Console.Write("Enter Library Code of item to return: ");
            string libraryCode = Console.ReadLine();

            receptionService.ReturnItem(libraryCode);
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        private static void RunReceptionReportsMenu()
        {
            bool inReportsMenu = true;

            while (inReportsMenu)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                  REPORTS                                 ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("1. View All Borrow Records");
                Console.WriteLine("2. View Overdue Items");
                Console.WriteLine("3. View Active Borrows");
                Console.WriteLine("4. View Borrower History");
                Console.WriteLine("5. Back");
                Console.Write("\nSelect an option (1-5): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        receptionService.DisplayAllBorrowRecords();
                        PressAnyKeyToContinue();
                        break;
                    case "2":
                        receptionService.DisplayOverdueItems();
                        PressAnyKeyToContinue();
                        break;
                    case "3":
                        DisplayActiveBorrows();
                        PressAnyKeyToContinue();
                        break;
                    case "4":
                        ReceptionViewBorrowerHistory();
                        break;
                    case "5":
                        inReportsMenu = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.\n");
                        PressAnyKeyToContinue();
                        break;
                }
            }
        }

        private static void DisplayActiveBorrows()
        {
            var activeBorrows = receptionService.GetActiveBorrows();

            if (activeBorrows.Count == 0)
            {
                Console.WriteLine("\nNo active borrows.");
                return;
            }

            Console.WriteLine("\n--- Active Borrows ---");
            for (int i = 0; i < activeBorrows.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {activeBorrows[i]}");
            }
        }

        private static void ReceptionViewBorrowerHistory()
        {
            Console.Write("\nEnter Borrower ID: ");
            string borrowerId = Console.ReadLine();

            var history = receptionService.GetBorrowerHistory(borrowerId);
            if (history.Count > 0)
            {
                Console.WriteLine($"\n--- Borrow History for {borrowerId} ---");
                for (int i = 0; i < history.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {history[i]}");
                }
            }
            Console.WriteLine();
            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Manager role menu
        /// </summary>
        private static void RunManagerRole()
        {
            bool inManagerMenu = true;

            while (inManagerMenu)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              MANAGER - STATISTICS & REPORTS              ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("1. Borrowing Statistics");
                Console.WriteLine("2. Item Status Statistics");
                Console.WriteLine("3. Fine Statistics");
                Console.WriteLine("4. Comprehensive Statistics");
                Console.WriteLine("5. Detailed Reports & Analysis");
                Console.WriteLine("6. Return to Main Menu");
                Console.Write("\nSelect an option (1-6): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        managerService.DisplayBorrowingStatistics();
                        PressAnyKeyToContinue();
                        break;
                    case "2":
                        managerService.DisplayItemStatusStatistics();
                        PressAnyKeyToContinue();
                        break;
                    case "3":
                        managerService.DisplayFineStatistics();
                        PressAnyKeyToContinue();
                        break;
                    case "4":
                        managerService.DisplayComprehensiveStatistics();
                        PressAnyKeyToContinue();
                        break;
                    case "5":
                        RunManagerDetailedReports();
                        break;
                    case "6":
                        inManagerMenu = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.\n");
                        PressAnyKeyToContinue();
                        break;
                }
            }
        }

        private static void RunManagerDetailedReports()
        {
            bool inDetailedReports = true;

            while (inDetailedReports)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║            DETAILED REPORTS & ANALYSIS                   ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("1. Most Borrowed Items");
                Console.WriteLine("2. Item Inventory Report");
                Console.WriteLine("3. Fine Collection Report");
                Console.WriteLine("4. Borrower Activity Report");
                Console.WriteLine("5. Overdue Items Report");
                Console.WriteLine("6. Back");
                Console.Write("\nSelect an option (1-6): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayMostBorrowedItems();
                        PressAnyKeyToContinue();
                        break;
                    case "2":
                        DisplayItemInventoryReport();
                        PressAnyKeyToContinue();
                        break;
                    case "3":
                        DisplayFineCollectionReport();
                        PressAnyKeyToContinue();
                        break;
                    case "4":
                        DisplayBorrowerActivityReport();
                        PressAnyKeyToContinue();
                        break;
                    case "5":
                        DisplayOverdueItemsReport();
                        PressAnyKeyToContinue();
                        break;
                    case "6":
                        inDetailedReports = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.\n");
                        PressAnyKeyToContinue();
                        break;
                }
            }
        }

        private static void DisplayMostBorrowedItems()
        {
            var stats = managerService.GetBorrowingStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         MOST BORROWED ITEMS REPORT                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            if (stats.MostBorrowedItems.Count == 0)
            {
                Console.WriteLine("\nNo borrow records yet.");
                return;
            }

            Console.WriteLine("\n  Ranking | Item Name                       | Times Borrowed");
            Console.WriteLine("  " + new string('-', 58));

            for (int i = 0; i < stats.MostBorrowedItems.Count; i++)
            {
                var item = stats.MostBorrowedItems[i];
                string itemDisplay = item.ItemName.Length > 28
                    ? item.ItemName.Substring(0, 25) + "..."
                    : item.ItemName;

                Console.WriteLine($"  {i + 1,-7} | {itemDisplay,-30} | {item.BorrowCount}");
            }

            Console.WriteLine();
        }

        private static void DisplayItemInventoryReport()
        {
            var stats = managerService.GetItemStatusStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         ITEM INVENTORY REPORT                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            Console.WriteLine($"\n  Total Items: {stats.TotalItemsCount}\n");
            Console.WriteLine("  Status Summary:");
            Console.WriteLine("  " + new string('-', 36));

            var statusLabels = new[] { ("Available", "✓", ConsoleColor.Green),
                                       ("Borrowed", "◆", ConsoleColor.Yellow),
                                       ("Damaged", "✗", ConsoleColor.Red),
                                       ("Destroy", "◉", ConsoleColor.DarkRed) };

            foreach (var (status, icon, color) in statusLabels)
            {
                int count = stats.StatusBreakdown.GetValueOrDefault(status, 0);
                double percentage = stats.TotalItemsCount > 0
                    ? Math.Round((double)count / stats.TotalItemsCount * 100, 1)
                    : 0;

                Console.ForegroundColor = color;
                Console.Write($"  {icon} ");
                Console.ResetColor();
                Console.WriteLine($"{status,-12}: {count,3} items ({percentage,5}%)");
            }

            Console.WriteLine();
        }

        private static void DisplayFineCollectionReport()
        {
            var stats = managerService.GetFineStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         FINE COLLECTION REPORT                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            Console.WriteLine($"\n  Completed Transactions:          {stats.CompletedBorrowsCount}");
            Console.WriteLine($"  Late Returns:                    {stats.LateReturnCount}");

            double lateReturnRate = stats.CompletedBorrowsCount > 0
                ? Math.Round((double)stats.LateReturnCount / stats.CompletedBorrowsCount * 100, 1)
                : 0;

            Console.WriteLine($"  Late Return Rate:                {lateReturnRate}%");

            Console.WriteLine($"\n  Financial Summary:");
            Console.WriteLine("  " + new string('-', 36));
            Console.WriteLine($"  Total Fines Collected:           ${stats.TotalFinesCollected:F2}");
            Console.WriteLine($"  Average Fine Per Item:           ${stats.AverageFine:F2}");
            Console.WriteLine($"  Current Overdue Amount:          ${stats.CurrentOverdueAmount:F2}");

            Console.WriteLine($"\n  Outstanding Issues:");
            Console.WriteLine("  " + new string('-', 36));
            Console.WriteLine($"  Items Still Overdue:             {stats.TotalOverdueItems}");
            Console.WriteLine($"  Potential Fine Revenue:          ${stats.CurrentOverdueAmount:F2}");

            if (stats.TotalOverdueItems > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  ⚠ Action required: {stats.TotalOverdueItems} item(s) overdue with ${stats.CurrentOverdueAmount:F2} in fines");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        private static void DisplayBorrowerActivityReport()
        {
            var borrowingStats = managerService.GetBorrowingStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         BORROWER ACTIVITY REPORT                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            Console.WriteLine($"\n  Total Borrowers:                 {Borrower.AllBorrowers.Count}");
            Console.WriteLine($"  Currently Active Borrows:        {borrowingStats.CurrentlyBorrowedCount}");
            Console.WriteLine($"  Total Transaction History:       {borrowingStats.TotalBorrowsCount}");

            double borrowerEngagement = Borrower.AllBorrowers.Count > 0
                ? Math.Round((double)borrowingStats.CurrentlyBorrowedCount / Borrower.AllBorrowers.Count * 100, 1)
                : 0;

            Console.WriteLine($"\n  Borrower Engagement:");
            Console.WriteLine("  " + new string('-', 36));
            Console.WriteLine($"  % of Borrowers with Active Items: {borrowerEngagement}%");

            if (borrowingStats.MostBorrowedItems.Count > 0)
            {
                Console.WriteLine($"\n  Average Borrows Per Borrower:    " +
                    $"{(double)borrowingStats.TotalBorrowsCount / (Borrower.AllBorrowers.Count > 0 ? Borrower.AllBorrowers.Count : 1):F2}");
            }

            Console.WriteLine();
        }

        private static void DisplayOverdueItemsReport()
        {
            var overdueItems = BorrowRecord.AllBorrowRecords
                .Where(br => !br.ReturnDate.HasValue && br.IsOverdue())
                .OrderByDescending(br => br.CalculateFine())
                .ToList();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         OVERDUE ITEMS REPORT                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            if (overdueItems.Count == 0)
            {
                Console.WriteLine("\n✓ Great news! No overdue items.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine($"\n  Total Overdue Items: {overdueItems.Count}\n");
            Console.WriteLine("  Item Code | Days Overdue | Borrower ID | Current Fine");
            Console.WriteLine("  " + new string('-', 58));

            decimal totalOverdueFines = 0;

            foreach (var record in overdueItems)
            {
                int daysOverdue = record.GetDaysBorrowed() - 14;
                decimal fine = record.CalculateFine();
                totalOverdueFines += fine;

                Console.Write($"  {record.LibraryCode,-9} | {daysOverdue,-12} | {record.BorrowerId,-11} | ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"${fine:F2}");
                Console.ResetColor();
            }

            Console.WriteLine("  " + new string('-', 58));
            Console.WriteLine($"  Total Overdue Fines:                      ${totalOverdueFines:F2}");

            if (overdueItems.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n  ⚠ WARNING: {overdueItems.Count} item(s) overdue. Immediate action recommended!");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Helper methods
        /// </summary>
        private static void DisplayWelcomeHeader()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      LIBRARY MANAGEMENT SYSTEM - ROLE SELECTION           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
        }

        private static void DisplayRoleMenu()
        {
            Console.WriteLine("Select Your Role:");
            Console.WriteLine("1. Admin        (Item Management - Create, Read, Update, Delete)");
            Console.WriteLine("2. Reception    (Borrower & Checkout Management)");
            Console.WriteLine("3. Manager      (Statistics & Analytics)");
            Console.WriteLine("4. Save Data    (Save all data to JSON files)");
            Console.WriteLine("5. Load Data    (Load all data from JSON files)");
            Console.WriteLine("6. Exit         (Close the application)");
            Console.Write("\nEnter your choice (1-6): ");
        }

        private static UserRole ParseRoleSelection(string choice)
        {
            return choice switch
            {
                "1" => UserRole.Admin,
                "2" => UserRole.Reception,
                "3" => UserRole.Manager,
                "4" => UserRole.SaveData,
                "5" => UserRole.LoadData,
                "6" => UserRole.Exit,
                _ => (UserRole)(-1)
            };
        }

        private static void DisplayGoodbyeMessage()
        {
            Console.Clear();
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("║   Thank you for using the Library Management System!      ║");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("║   All data has been preserved during this session.         ║");
            Console.WriteLine("║   Goodbye!                                                 ║");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
        }

        private static void PressAnyKeyToContinue()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Initialize sample data for the first run
        /// First attempts to load saved data, then initializes sample data if needed
        /// </summary>
        private static void InitializeSampleData()
        {
            // First, try to load saved data
            if (AttemptLoadData())
            {
                // Data was loaded successfully, skip sample initialization
                Console.WriteLine("Press any key to continue to the main menu...");
                Console.ReadKey();
                return;
            }

            // Only initialize sample data if data hasn't been loaded yet
            if (Item.AllItems.Count == 0)
            {
                Console.WriteLine("Initializing Library Management System...\n");

                // Create sample items
                adminService.CreateBook(
                    "The Great Gatsby",
                    "A classic American novel",
                    "BOOK001",
                    "Available",
                    "F. Scott Fitzgerald",
                    "Fiction"
                );

                adminService.CreateBook(
                    "To Kill a Mockingbird",
                    "An American literary classic",
                    "BOOK002",
                    "Available",
                    "Harper Lee",
                    "Fiction"
                );

                adminService.CreateMusic(
                    "Abbey Road",
                    "An album by The Beatles",
                    "MUSIC001",
                    "Available",
                    "The Beatles",
                    1969
                );

                adminService.CreateMusic(
                    "Rumours",
                    "An album by Fleetwood Mac",
                    "MUSIC002",
                    "Available",
                    "Fleetwood Mac",
                    1977
                );

                adminService.CreateToy(
                    "Building Blocks Set",
                    "Colorful building blocks for kids",
                    "TOY001",
                    "Available",
                    "Building Blocks",
                    "3-8"
                );

                adminService.CreateToy(
                    "Puzzle Game",
                    "An educational puzzle game",
                    "TOY002",
                    "Available",
                    "Puzzle",
                    "5-12"
                );

                // Create sample borrowers
                receptionService.CreateBorrower("BR001", "John Smith", "555-0101", "john.smith@email.com");
                receptionService.CreateBorrower("BR002", "Mary Johnson", "555-0102", "mary.johnson@email.com");
                receptionService.CreateBorrower("BR003", "Robert Brown", "555-0103", "robert.brown@email.com");

                // Create sample borrow records
                receptionService.BorrowItem("BOOK001", "BR001");
                receptionService.BorrowItem("MUSIC001", "BR002");

                Console.WriteLine("✓ Sample data initialized successfully!");
                Console.WriteLine($"✓ {Item.AllItems.Count} items loaded");
                Console.WriteLine($"✓ {Borrower.AllBorrowers.Count} borrowers loaded");
                Console.WriteLine($"✓ {BorrowRecord.AllBorrowRecords.Count} borrow records loaded\n");
            }

            Console.WriteLine("Press any key to continue to the main menu...");
            Console.ReadKey();
        }
    }
}
