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
            if (Assignment2.LibraryManagement.DataService.LoadAllData())
            {
                System.Console.WriteLine("✓ Loaded existing data from files");
                return;
            }

            if (Assignment2.LibraryManagement.Item.AllItems.Count == 0)
            {
                System.Console.WriteLine("Initializing sample data for web application...");
                InitializeSampleData();
                System.Console.WriteLine("✓ Sample data initialized");
            }
        }

        /// <summary>
        /// Initialize sample data for the library
        /// </summary>
        private static void InitializeSampleData()
        {
            var adminService = new Assignment2.LibraryManagement.AdminService();

            // Sample Books
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

            adminService.CreateBook(
                "1984",
                "A dystopian social science fiction novel",
                "BOOK003",
                "Borrowed",
                "George Orwell",
                "Science Fiction"
            );

            // Sample Music
            adminService.CreateMusic(
                "White Album",
                "The Beatles' iconic album",
                "MUSIC001",
                "Available",
                "The Beatles",
                1968
            );

            adminService.CreateMusic(
                "Dark Side of the Moon",
                "Pink Floyd's masterpiece",
                "MUSIC002",
                "Available",
                "Pink Floyd",
                1973
            );

            // Sample Toys
            adminService.CreateToy(
                "LEGO Classic Set",
                "Educational building blocks",
                "TOY001",
                "Available",
                "Construction",
                "4-12"
            );

            adminService.CreateToy(
                "Wooden Puzzle",
                "Traditional wooden puzzle game",
                "TOY002",
                "Borrowed",
                "Puzzle",
                "3-7"
            );
        }
    }
}
