using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service for persisting and loading all library data to/from JSON files
    /// Handles serialization of Items (Book, Music, Toy), Borrowers, and BorrowRecords
    /// Uses System.Text.Json with custom converters for polymorphic types
    /// </summary>
    public static class DataService
    {
        // Data directory and file paths
        private static readonly string DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        private static readonly string ItemsFilePath = Path.Combine(DataDirectory, "items.json");
        private static readonly string BorrowersFilePath = Path.Combine(DataDirectory, "borrowers.json");
        private static readonly string BorrowRecordsFilePath = Path.Combine(DataDirectory, "borrow_records.json");
        private static readonly string ReservationsFilePath = Path.Combine(DataDirectory, "reservations.json");
        private static readonly string WaitlistEntriesFilePath = Path.Combine(DataDirectory, "waitlist_entries.json");
        private static readonly string NotificationsFilePath = Path.Combine(DataDirectory, "notifications.json");

        // JSON serializer options with custom converters
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ItemJsonConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Saves all data collections to JSON files
        /// Creates data directory if it doesn't exist
        /// Returns true if save was successful, false otherwise
        /// </summary>
        public static bool SaveAllData()
        {
            try
            {
                // Create data directory if it doesn't exist
                if (!Directory.Exists(DataDirectory))
                {
                    Directory.CreateDirectory(DataDirectory);
                }

                // Save items
                SaveItems();
                Console.WriteLine($"✓ Items saved to {ItemsFilePath}");

                // Save borrowers
                SaveBorrowers();
                Console.WriteLine($"✓ Borrowers saved to {BorrowersFilePath}");

                // Save borrow records
                SaveBorrowRecords();
                Console.WriteLine($"✓ Borrow records saved to {BorrowRecordsFilePath}");

                // Save reservations
                SaveReservations();
                Console.WriteLine($"✓ Reservations saved to {ReservationsFilePath}");

                // Save waitlist entries
                SaveWaitlistEntries();
                Console.WriteLine($"✓ Waitlist entries saved to {WaitlistEntriesFilePath}");

                // Save notifications
                SaveNotifications();
                Console.WriteLine($"✓ Notifications saved to {NotificationsFilePath}");

                Console.WriteLine("\n✓ All data saved successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error saving data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads all data collections from JSON files
        /// Clears existing collections before loading
        /// Returns true if load was successful, false otherwise
        /// </summary>
        public static bool LoadAllData()
        {
            // If no data files exist, return silently (first run)
            if (!File.Exists(ItemsFilePath) && 
                !File.Exists(BorrowersFilePath) && 
                !File.Exists(BorrowRecordsFilePath) &&
                !File.Exists(ReservationsFilePath) &&
                !File.Exists(WaitlistEntriesFilePath) &&
                !File.Exists(NotificationsFilePath))
            {
                return false; // No data to load
            }

            try
            {
                // Load items
                if (File.Exists(ItemsFilePath))
                {
                    LoadItems();
                    Console.WriteLine($"✓ Items loaded from {ItemsFilePath}");
                }

                // Load borrowers
                if (File.Exists(BorrowersFilePath))
                {
                    LoadBorrowers();
                    Console.WriteLine($"✓ Borrowers loaded from {BorrowersFilePath}");
                }

                // Load borrow records
                if (File.Exists(BorrowRecordsFilePath))
                {
                    LoadBorrowRecords();
                    Console.WriteLine($"✓ Borrow records loaded from {BorrowRecordsFilePath}");
                }

                // Load reservations
                if (File.Exists(ReservationsFilePath))
                {
                    LoadReservations();
                    Console.WriteLine($"✓ Reservations loaded from {ReservationsFilePath}");
                }

                // Load waitlist entries
                if (File.Exists(WaitlistEntriesFilePath))
                {
                    LoadWaitlistEntries();
                    Console.WriteLine($"✓ Waitlist entries loaded from {WaitlistEntriesFilePath}");
                }

                // Load notifications
                if (File.Exists(NotificationsFilePath))
                {
                    LoadNotifications();
                    Console.WriteLine($"✓ Notifications loaded from {NotificationsFilePath}");
                }

                Console.WriteLine("\n✓ All data loaded successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error loading data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load and clear summary - shows statistics without loading data
        /// </summary>
        public static void DisplayDataStats()
        {
            Console.WriteLine("\n=== Current Data Statistics ===");
            Console.WriteLine($"📚 Items in memory: {Item.AllItems.Count}");
            Console.WriteLine($"👤 Borrowers in memory: {Borrower.AllBorrowers.Count}");
            Console.WriteLine($"📋 Borrow records in memory: {BorrowRecord.AllBorrowRecords.Count}");
            Console.WriteLine($"📌 Reservations in memory: {Reservation.AllReservations.Count}");
            Console.WriteLine($"⏳ Waitlist entries in memory: {WaitlistEntry.AllWaitlistEntries.Count}");
            Console.WriteLine($"🔔 Notifications in memory: {Notification.AllNotifications.Count}");

            if (File.Exists(ItemsFilePath))
            {
                try
                {
                    int savedItems = CountItemsInFile(ItemsFilePath);
                    Console.WriteLine($"💾 Items in saved file: {savedItems}");
                }
                catch { }
            }

            if (File.Exists(BorrowersFilePath))
            {
                try
                {
                    int savedBorrowers = CountBorrowersInFile(BorrowersFilePath);
                    Console.WriteLine($"💾 Borrowers in saved file: {savedBorrowers}");
                }
                catch { }
            }

            if (File.Exists(BorrowRecordsFilePath))
            {
                try
                {
                    int savedRecords = CountRecordsInFile(BorrowRecordsFilePath);
                    Console.WriteLine($"💾 Borrow records in saved file: {savedRecords}");
                }
                catch { }
            }

            if (File.Exists(ReservationsFilePath))
            {
                try
                {
                    int savedReservations = CountReservationsInFile(ReservationsFilePath);
                    Console.WriteLine($"💾 Reservations in saved file: {savedReservations}");
                }
                catch { }
            }

            if (File.Exists(WaitlistEntriesFilePath))
            {
                try
                {
                    int savedWaitlistEntries = CountWaitlistEntriesInFile(WaitlistEntriesFilePath);
                    Console.WriteLine($"💾 Waitlist entries in saved file: {savedWaitlistEntries}");
                }
                catch { }
            }

            if (File.Exists(NotificationsFilePath))
            {
                try
                {
                    int savedNotifications = CountNotificationsInFile(NotificationsFilePath);
                    Console.WriteLine($"💾 Notifications in saved file: {savedNotifications}");
                }
                catch { }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Clear all data from memory (for testing or full reset)
        /// </summary>
        public static void ClearAllData()
        {
            Item.AllItems.Clear();
            Borrower.AllBorrowers.Clear();
            BorrowRecord.AllBorrowRecords.Clear();
            Reservation.AllReservations.Clear();
            WaitlistEntry.AllWaitlistEntries.Clear();
            Notification.AllNotifications.Clear();
            Console.WriteLine("✓ All data cleared from memory");
        }

        /// <summary>
        /// Check if saved data files exist
        /// </summary>
        public static bool HasSavedData()
        {
            return File.Exists(ItemsFilePath) || 
                   File.Exists(BorrowersFilePath) || 
                   File.Exists(BorrowRecordsFilePath) ||
                   File.Exists(ReservationsFilePath) ||
                   File.Exists(WaitlistEntriesFilePath) ||
                   File.Exists(NotificationsFilePath);
        }

        // ===== Private Helper Methods =====

        /// <summary>
        /// Save items collection to JSON file
        /// </summary>
        private static void SaveItems()
        {
            var itemsDto = Item.AllItems.Select(item => item).ToList();
            string json = JsonSerializer.Serialize(itemsDto, JsonOptions);
            File.WriteAllText(ItemsFilePath, json);
        }

        /// <summary>
        /// Load items collection from JSON file
        /// </summary>
        private static void LoadItems()
        {
            Item.AllItems.Clear();
            string json = File.ReadAllText(ItemsFilePath);
            var items = JsonSerializer.Deserialize<List<Item>>(json, JsonOptions);

            if (items != null)
            {
                foreach (var item in items)
                {
                    Item.AllItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Save borrowers collection to JSON file
        /// </summary>
        private static void SaveBorrowers()
        {
            string json = JsonSerializer.Serialize(Borrower.AllBorrowers, JsonOptions);
            File.WriteAllText(BorrowersFilePath, json);
        }

        /// <summary>
        /// Load borrowers collection from JSON file
        /// </summary>
        private static void LoadBorrowers()
        {
            Borrower.AllBorrowers.Clear();
            string json = File.ReadAllText(BorrowersFilePath);
            var borrowers = JsonSerializer.Deserialize<List<Borrower>>(json, JsonOptions);

            if (borrowers != null)
            {
                foreach (var borrower in borrowers)
                {
                    Borrower.AllBorrowers.Add(borrower);
                }
            }
        }

        /// <summary>
        /// Save borrow records collection to JSON file
        /// </summary>
        private static void SaveBorrowRecords()
        {
            string json = JsonSerializer.Serialize(BorrowRecord.AllBorrowRecords, JsonOptions);
            File.WriteAllText(BorrowRecordsFilePath, json);
        }

        /// <summary>
        /// Load borrow records collection from JSON file
        /// </summary>
        private static void LoadBorrowRecords()
        {
            BorrowRecord.AllBorrowRecords.Clear();
            string json = File.ReadAllText(BorrowRecordsFilePath);
            var records = JsonSerializer.Deserialize<List<BorrowRecord>>(json, JsonOptions);

            if (records != null)
            {
                foreach (var record in records)
                {
                    BorrowRecord.AllBorrowRecords.Add(record);
                }
            }
        }

        /// <summary>
        /// Count items in saved JSON file (for statistics)
        /// </summary>
        private static int CountItemsInFile(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            string json = File.ReadAllText(filePath);
            var items = JsonSerializer.Deserialize<List<Item>>(json, JsonOptions);
            return items?.Count ?? 0;
        }

        /// <summary>
        /// Count borrowers in saved JSON file (for statistics)
        /// </summary>
        private static int CountBorrowersInFile(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            string json = File.ReadAllText(filePath);
            var borrowers = JsonSerializer.Deserialize<List<Borrower>>(json, JsonOptions);
            return borrowers?.Count ?? 0;
        }

        /// <summary>
        /// Count records in saved JSON file (for statistics)
        /// </summary>
        private static int CountRecordsInFile(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            string json = File.ReadAllText(filePath);
            var records = JsonSerializer.Deserialize<List<BorrowRecord>>(json, JsonOptions);
            return records?.Count ?? 0;
        }

        /// <summary>
        /// Save reservations collection to JSON file
        /// </summary>
        private static void SaveReservations()
        {
            string json = JsonSerializer.Serialize(Reservation.AllReservations, JsonOptions);
            File.WriteAllText(ReservationsFilePath, json);
        }

        /// <summary>
        /// Load reservations collection from JSON file
        /// </summary>
        private static void LoadReservations()
        {
            Reservation.AllReservations.Clear();
            string json = File.ReadAllText(ReservationsFilePath);
            var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, JsonOptions);

            if (reservations != null)
            {
                foreach (var reservation in reservations)
                {
                    Reservation.AllReservations.Add(reservation);
                }
            }
        }

        /// <summary>
        /// Count reservations in saved JSON file (for statistics)
        /// </summary>
        private static int CountReservationsInFile(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            string json = File.ReadAllText(filePath);
            var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, JsonOptions);
            return reservations?.Count ?? 0;
        }

        /// <summary>
        /// Save waitlist entries collection to JSON file
        /// </summary>
        private static void SaveWaitlistEntries()
        {
            string json = JsonSerializer.Serialize(WaitlistEntry.AllWaitlistEntries, JsonOptions);
            File.WriteAllText(WaitlistEntriesFilePath, json);
        }

        /// <summary>
        /// Load waitlist entries collection from JSON file
        /// </summary>
        private static void LoadWaitlistEntries()
        {
            WaitlistEntry.AllWaitlistEntries.Clear();
            string json = File.ReadAllText(WaitlistEntriesFilePath);
            var waitlistEntries = JsonSerializer.Deserialize<List<WaitlistEntry>>(json, JsonOptions);

            if (waitlistEntries != null)
            {
                foreach (var entry in waitlistEntries)
                {
                    WaitlistEntry.AllWaitlistEntries.Add(entry);
                }
            }
        }

        /// <summary>
        /// Count waitlist entries in saved JSON file (for statistics)
        /// </summary>
        private static int CountWaitlistEntriesInFile(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            string json = File.ReadAllText(filePath);
            var entries = JsonSerializer.Deserialize<List<WaitlistEntry>>(json, JsonOptions);
            return entries?.Count ?? 0;
        }

        /// <summary>
        /// Save notifications collection to JSON file
        /// </summary>
        private static void SaveNotifications()
        {
            string json = JsonSerializer.Serialize(Notification.AllNotifications, JsonOptions);
            File.WriteAllText(NotificationsFilePath, json);
        }

        /// <summary>
        /// Load notifications collection from JSON file
        /// </summary>
        private static void LoadNotifications()
        {
            Notification.AllNotifications.Clear();
            string json = File.ReadAllText(NotificationsFilePath);
            var notifications = JsonSerializer.Deserialize<List<Notification>>(json, JsonOptions);

            if (notifications != null)
            {
                foreach (var notification in notifications)
                {
                    Notification.AllNotifications.Add(notification);
                }
            }
        }

        /// <summary>
        /// Count notifications in saved JSON file (for statistics)
        /// </summary>
        private static int CountNotificationsInFile(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            string json = File.ReadAllText(filePath);
            var notifications = JsonSerializer.Deserialize<List<Notification>>(json, JsonOptions);
            return notifications?.Count ?? 0;
        }

        /// <summary>
        /// Delete all saved data files (for factory reset)
        /// </summary>
        public static bool DeleteSavedData()
        {
            try
            {
                if (File.Exists(ItemsFilePath)) File.Delete(ItemsFilePath);
                if (File.Exists(BorrowersFilePath)) File.Delete(BorrowersFilePath);
                if (File.Exists(BorrowRecordsFilePath)) File.Delete(BorrowRecordsFilePath);
                if (File.Exists(ReservationsFilePath)) File.Delete(ReservationsFilePath);
                if (File.Exists(WaitlistEntriesFilePath)) File.Delete(WaitlistEntriesFilePath);
                if (File.Exists(NotificationsFilePath)) File.Delete(NotificationsFilePath);

                if (Directory.Exists(DataDirectory) && Directory.GetFiles(DataDirectory).Length == 0)
                {
                    Directory.Delete(DataDirectory);
                }

                Console.WriteLine("✓ All saved data files deleted");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error deleting saved data: {ex.Message}");
                return false;
            }
        }
    }
}
