namespace Assignment2
{
    /// <summary>
    /// Web application startup helper
    /// Called from LibraryManagementConsole when --web argument is passed
    /// </summary>
    public class WebAppStartup
    {
        public static void RunWebApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                // In development, don't require HTTPS
                // Remove the app.UseHttpsRedirection() call
            }

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Initialize data for the web app
            using (var scope = app.Services.CreateScope())
            {
                InitializeWebAppData();
            }

            app.Run();
        }

        /// <summary>
        /// Initialize data for the web application
        /// </summary>
        private static void InitializeWebAppData()
        {
            bool loaded = Assignment2.LibraryManagement.DataService.LoadAllData();

            // Check which collections are empty and need initialization
            bool branchesEmpty = Assignment2.LibraryManagement.LibraryBranch.AllBranches.Count == 0;
            bool itemsEmpty = Assignment2.LibraryManagement.Item.AllItems.Count == 0;
            bool transfersEmpty = Assignment2.LibraryManagement.LibraryTransfer.AllTransfers.Count == 0;
            bool borrowersEmpty = Assignment2.LibraryManagement.Borrower.AllBorrowers.Count == 0;

            // If all collections are already populated from persisted data, return early
            if (loaded && !branchesEmpty && !itemsEmpty && !transfersEmpty && !borrowersEmpty)
            {
                System.Console.WriteLine("✓ Loaded existing data from files");
                return;
            }

            // Initialize sample branches if none exist
            if (branchesEmpty)
            {
                System.Console.WriteLine("Initializing sample branches...");
                InitializeSampleBranches();
                System.Console.WriteLine("✓ Sample branches initialized");
            }

            // Ensure sample items are initialized
            if (itemsEmpty)
            {
                System.Console.WriteLine("Initializing sample data for web application...");
                InitializeSampleData();
                System.Console.WriteLine("✓ Sample data initialized");
            }

            // Initialize sample transfers if none exist
            if (transfersEmpty)
            {
                System.Console.WriteLine("Initializing sample transfers...");
                InitializeSampleTransfers();
                System.Console.WriteLine("✓ Sample transfers initialized");
            }

            // Ensure sample borrowers are initialized
            if (borrowersEmpty)
            {
                System.Console.WriteLine("Initializing sample borrowers...");
                InitializeSampleBorrowers();
                System.Console.WriteLine("✓ Sample borrowers initialized");
            }

            // Save all initialized data to persist for future runs
            if (Assignment2.LibraryManagement.DataService.SaveAllData())
            {
                System.Console.WriteLine("✓ Data persisted to disk");
            }
        }

        /// <summary>
        /// Initialize sample data for the library
        /// </summary>
        private static void InitializeSampleData()
        {
            var adminService = new Assignment2.LibraryManagement.AdminService();

            // Sample Books - Distributed across branches
            // Downtown Library (BR001)
            adminService.CreateBook(
                "The Great Gatsby",
                "A classic American novel",
                "BOOK001",
                "Available",
                "F. Scott Fitzgerald",
                "Fiction",
                "BR001"
            );

            // Campus Branch (BR002)
            adminService.CreateBook(
                "To Kill a Mockingbird",
                "An American literary classic",
                "BOOK002",
                "Available",
                "Harper Lee",
                "Fiction",
                "BR002"
            );

            // Riverside Community Library (BR003)
            adminService.CreateBook(
                "1984",
                "A dystopian social science fiction novel",
                "BOOK003",
                "Borrowed",
                "George Orwell",
                "Science Fiction",
                "BR003"
            );

            // Sample Music - Distributed across branches
            // Downtown Library (BR001)
            adminService.CreateMusic(
                "White Album",
                "The Beatles' iconic album",
                "MUSIC001",
                "Available",
                "The Beatles",
                1968,
                "BR001"
            );

            // Campus Branch (BR002)
            adminService.CreateMusic(
                "Dark Side of the Moon",
                "Pink Floyd's masterpiece",
                "MUSIC002",
                "Available",
                "Pink Floyd",
                1973,
                "BR002"
            );

            // Sample Toys - Distributed across branches
            // Riverside Community Library (BR003)
            adminService.CreateToy(
                "LEGO Classic Set",
                "Educational building blocks",
                "TOY001",
                "Available",
                "Construction",
                "4-12",
                "BR003"
            );

            // Downtown Library (BR001)
            adminService.CreateToy(
                "Wooden Puzzle",
                "Traditional wooden puzzle game",
                "TOY002",
                "Borrowed",
                "Puzzle",
                "3-7",
                "BR001"
            );

            adminService.CreateToy(
                "Wooden Puzzle",
                "Traditional wooden puzzle game",
                "TOY002",
                "Borrowed",
                "Puzzle",
                "3-7"
            );

            // Sample borrowers so the self-service kiosk checkout workflow can
            // be exercised in web mode (mirrors the console sample data).
            var receptionService = new Assignment2.LibraryManagement.ReceptionService();
            receptionService.CreateBorrower("BR001", "John Smith", "555-0101", "john.smith@email.com");
            receptionService.CreateBorrower("BR002", "Mary Johnson", "555-0102", "mary.johnson@email.com");
            receptionService.CreateBorrower("BR003", "Robert Brown", "555-0103", "robert.brown@email.com");
        }

        /// <summary>
        /// Initialize sample library branches
        /// Creates three realistic branches: Downtown, Campus, and Riverside
        /// Branches are only created if AllBranches is empty (idempotent)
        /// </summary>
        private static void InitializeSampleBranches()
        {
            var branchService = new Assignment2.LibraryManagement.BranchService();

            // Branch 1: Downtown Library
            branchService.CreateBranch(
                "BR001",
                "Downtown Library",
                "123 Main Street, City Center | Tel: (555) 001-1234 | Hours: 9 AM - 9 PM"
            );

            // Branch 2: Campus Branch
            branchService.CreateBranch(
                "BR002",
                "Campus Branch",
                "456 University Ave, Academic District | Tel: (555) 002-5678 | Hours: 8 AM - 10 PM"
            );

            // Branch 3: Riverside Community Library
            branchService.CreateBranch(
                "BR003",
                "Riverside Community Library",
                "789 Riverside Road, Suburban Area | Tel: (555) 003-9999 | Hours: 10 AM - 6 PM"
            );
        }

        /// <summary>
        /// Initialize sample transfers for demonstration
        /// Creates transfers in various states: one completed, one pending approval, one in transit
        /// Only runs on first app startup (when transfers.json doesn't exist)
        /// </summary>
        private static void InitializeSampleTransfers()
        {
            // Only seed if no transfers exist
            if (Assignment2.LibraryManagement.LibraryTransfer.AllTransfers.Count > 0)
                return;

            // Sample Transfer 1: Completed transfer (BOOK001 from BR001 to BR002)
            var transfer1 = Assignment2.LibraryManagement.TransferService.InitiateTransfer("BOOK001", "BR001", "BR002", "Requested by Campus library");
            Assignment2.LibraryManagement.TransferService.ApproveTransfer(transfer1.TransferId, "BR002", "Received and verified");
            Assignment2.LibraryManagement.TransferService.CompleteTransfer(transfer1.TransferId, "BR002");

            // Sample Transfer 2: Pending Approval (MUSIC001 from BR001 to BR003)
            var transfer2 = Assignment2.LibraryManagement.TransferService.InitiateTransfer("MUSIC001", "BR001", "BR003", "High demand at Riverside");
            // transfer2 remains in "Initiated" status awaiting BR003 approval

            // Sample Transfer 3: In Transit (MUSIC002 from BR002 to BR001)
            var transfer3 = Assignment2.LibraryManagement.TransferService.InitiateTransfer("MUSIC002", "BR002", "BR001", "Balanced collection distribution");
            Assignment2.LibraryManagement.TransferService.ApproveTransfer(transfer3.TransferId, "BR001", "Approved for receipt");
            // transfer3 remains in "Approved" status (simulating in-transit)

            System.Console.WriteLine("✓ Sample transfers initialized");
        }

        /// <summary>
        /// Initialize sample borrowers for the web application
        /// Creates three standard borrowers if none exist
        /// </summary>
        private static void InitializeSampleBorrowers()
        {
            var receptionService = new Assignment2.LibraryManagement.ReceptionService();

            receptionService.CreateBorrower("BR001", "John Smith", "555-0101", "john.smith@email.com");
            receptionService.CreateBorrower("BR002", "Mary Johnson", "555-0102", "mary.johnson@email.com");
            receptionService.CreateBorrower("BR003", "Robert Brown", "555-0103", "robert.brown@email.com");
        }
    }
}

