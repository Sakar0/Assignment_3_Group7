using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service class for generating statistics and reports about the library
    /// </summary>
    public class ManagerService
    {
        /// <summary>
        /// Gets borrowing statistics
        /// </summary>
        public BorrowingStatistics GetBorrowingStatistics()
        {
            var activeBorrows = BorrowRecord.AllBorrowRecords
                .Where(br => !br.ReturnDate.HasValue)
                .ToList();

            var totalBorrows = BorrowRecord.AllBorrowRecords.Count;

            var mostBorrowedItems = BorrowRecord.AllBorrowRecords
                .GroupBy(br => br.LibraryCode)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Take(5)
                .Select(g => new MostBorrowedItem
                {
                    LibraryCode = g.Key,
                    BorrowCount = g.Count(),
                    ItemName = GetItemNameByCode(g.Key)
                })
                .ToList();

            return new BorrowingStatistics
            {
                CurrentlyBorrowedCount = activeBorrows.Count,
                TotalBorrowsCount = totalBorrows,
                MostBorrowedItems = mostBorrowedItems
            };
        }

        /// <summary>
        /// Gets item status statistics
        /// </summary>
        public ItemStatusStatistics GetItemStatusStatistics()
        {
            var statusCounts = Item.AllItems
                .GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all status values are represented
            var allStatuses = new[] { "Available", "Borrowed", "Damaged", "Destroy" };
            foreach (var status in allStatuses)
            {
                if (!statusCounts.ContainsKey(status))
                {
                    statusCounts[status] = 0;
                }
            }

            return new ItemStatusStatistics
            {
                TotalItemsCount = Item.AllItems.Count,
                AvailableCount = statusCounts.GetValueOrDefault("Available", 0),
                BorrowedCount = statusCounts.GetValueOrDefault("Borrowed", 0),
                DamagedCount = statusCounts.GetValueOrDefault("Damaged", 0),
                DestroyCount = statusCounts.GetValueOrDefault("Destroy", 0),
                StatusBreakdown = statusCounts
            };
        }

        /// <summary>
        /// Gets fine statistics
        /// </summary>
        public FineStatistics GetFineStatistics()
        {
            var collectedFines = BorrowRecord.AllBorrowRecords
                .Where(br => br.ReturnDate.HasValue)
                .ToList();

            var lateReturns = collectedFines
                .Where(br => br.FineAmount > 0)
                .ToList();

            decimal totalFines = collectedFines.Sum(br => br.FineAmount);
            decimal averageFine = collectedFines.Count > 0 
                ? totalFines / collectedFines.Count 
                : 0;

            var currentOverdueFines = BorrowRecord.AllBorrowRecords
                .Where(br => !br.ReturnDate.HasValue && br.IsOverdue())
                .Sum(br => br.CalculateFine());

            return new FineStatistics
            {
                TotalFinesCollected = totalFines,
                LateReturnCount = lateReturns.Count,
                AverageFine = averageFine,
                CompletedBorrowsCount = collectedFines.Count,
                CurrentOverdueAmount = currentOverdueFines,
                TotalOverdueItems = BorrowRecord.AllBorrowRecords
                    .Count(br => !br.ReturnDate.HasValue && br.IsOverdue())
            };
        }

        /// <summary>
        /// Gets comprehensive library statistics
        /// </summary>
        public LibraryStatistics GetComprehensiveStatistics()
        {
            return new LibraryStatistics
            {
                BorrowingStats = GetBorrowingStatistics(),
                ItemStatusStats = GetItemStatusStatistics(),
                FineStats = GetFineStatistics(),
                TotalBorrowers = Borrower.AllBorrowers.Count,
                TotalItems = Item.AllItems.Count
            };
        }

        /// <summary>
        /// Gets the item name by library code
        /// </summary>
        private string GetItemNameByCode(string libraryCode)
        {
            var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == libraryCode);
            return item?.Name ?? "Unknown Item";
        }

        /// <summary>
        /// Displays borrowing statistics in a formatted way
        /// </summary>
        public void DisplayBorrowingStatistics()
        {
            var stats = GetBorrowingStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         BORROWING STATISTICS                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            Console.WriteLine($"\n  Currently Borrowed Items:        {stats.CurrentlyBorrowedCount}");
            Console.WriteLine($"  Total Borrow Transactions:       {stats.TotalBorrowsCount}");

            if (stats.MostBorrowedItems.Count > 0)
            {
                Console.WriteLine("\n  Most Borrowed Items (Top 5):");
                for (int i = 0; i < stats.MostBorrowedItems.Count; i++)
                {
                    var item = stats.MostBorrowedItems[i];
                    Console.WriteLine($"    {i + 1}. {item.ItemName}");
                    Console.WriteLine($"       (Code: {item.LibraryCode}, Borrowed: {item.BorrowCount} times)");
                }
            }
            else
            {
                Console.WriteLine("\n  No borrow records yet.");
            }
        }

        /// <summary>
        /// Displays item status statistics in a formatted way
        /// </summary>
        public void DisplayItemStatusStatistics()
        {
            var stats = GetItemStatusStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         ITEM STATUS STATISTICS                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            Console.WriteLine($"\n  Total Items in Library:          {stats.TotalItemsCount}");
            Console.WriteLine("\n  Items by Status:");
            Console.WriteLine($"    ✓ Available:                    {stats.AvailableCount} ({GetPercentage(stats.AvailableCount, stats.TotalItemsCount)}%)");
            Console.WriteLine($"    ◆ Borrowed:                     {stats.BorrowedCount} ({GetPercentage(stats.BorrowedCount, stats.TotalItemsCount)}%)");
            Console.WriteLine($"    ✗ Damaged:                      {stats.DamagedCount} ({GetPercentage(stats.DamagedCount, stats.TotalItemsCount)}%)");
            Console.WriteLine($"    ◉ Destroy:                      {stats.DestroyCount} ({GetPercentage(stats.DestroyCount, stats.TotalItemsCount)}%)");

            DisplayStatusBar(stats);
        }

        /// <summary>
        /// Displays fine statistics in a formatted way
        /// </summary>
        public void DisplayFineStatistics()
        {
            var stats = GetFineStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         FINE STATISTICS                                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            Console.WriteLine($"\n  Completed Borrow Transactions:   {stats.CompletedBorrowsCount}");
            Console.WriteLine($"  Late Returns:                    {stats.LateReturnCount}");
            Console.WriteLine($"    Late Return Rate:              {GetPercentage(stats.LateReturnCount, stats.CompletedBorrowsCount)}%");
            Console.WriteLine($"\n  Total Fines Collected:           ${stats.TotalFinesCollected:F2}");
            Console.WriteLine($"  Average Fine Per Item:           ${stats.AverageFine:F2}");

            Console.WriteLine($"\n  Overdue Items (Still Borrowed):  {stats.TotalOverdueItems}");
            Console.WriteLine($"  Current Overdue Amount:          ${stats.CurrentOverdueAmount:F2}");
        }

        /// <summary>
        /// Displays comprehensive statistics
        /// </summary>
        public void DisplayComprehensiveStatistics()
        {
            var stats = GetComprehensiveStatistics();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      COMPREHENSIVE LIBRARY STATISTICS                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            Console.WriteLine($"\n  Total Items in Library:          {stats.TotalItems}");
            Console.WriteLine($"  Total Registered Borrowers:      {stats.TotalBorrowers}");
            Console.WriteLine($"  Currently Borrowed Items:        {stats.BorrowingStats.CurrentlyBorrowedCount}");
            Console.WriteLine($"  Total Borrow Transactions:       {stats.BorrowingStats.TotalBorrowsCount}");

            DisplayBorrowingStatistics();
            DisplayItemStatusStatistics();
            DisplayFineStatistics();
        }

        /// <summary>
        /// Helper method to calculate percentage
        /// </summary>
        private double GetPercentage(int value, int total)
        {
            if (total == 0) return 0;
            return Math.Round((double)value / total * 100, 1);
        }

        /// <summary>
        /// Displays a visual status bar
        /// </summary>
        private void DisplayStatusBar(ItemStatusStatistics stats)
        {
            Console.WriteLine("\n  Visual Breakdown:");

            int barLength = 40;
            Console.Write("    [");

            int availableBar = (int)Math.Round((double)stats.AvailableCount / stats.TotalItemsCount * barLength);
            int borrowedBar = (int)Math.Round((double)stats.BorrowedCount / stats.TotalItemsCount * barLength);
            int damagedBar = (int)Math.Round((double)stats.DamagedCount / stats.TotalItemsCount * barLength);
            int destroyBar = (int)Math.Round((double)stats.DestroyCount / stats.TotalItemsCount * barLength);

            // Adjust for rounding errors
            int total = availableBar + borrowedBar + damagedBar + destroyBar;
            if (total < barLength)
            {
                availableBar += barLength - total;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('█', availableBar));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(new string('█', borrowedBar));

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(new string('█', damagedBar));

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(new string('█', destroyBar));

            Console.ResetColor();
            Console.WriteLine("]");

            Console.WriteLine($"    Available(Green) | Borrowed(Yellow) | Damaged(Red) | Destroy(DarkRed)");
        }
    }

    /// <summary>
    /// Contains borrowing statistics
    /// </summary>
    public class BorrowingStatistics
    {
        public int CurrentlyBorrowedCount { get; set; }
        public int TotalBorrowsCount { get; set; }
        public List<MostBorrowedItem> MostBorrowedItems { get; set; } = new List<MostBorrowedItem>();
    }

    /// <summary>
    /// Represents a most borrowed item
    /// </summary>
    public class MostBorrowedItem
    {
        public string LibraryCode { get; set; }
        public string ItemName { get; set; }
        public int BorrowCount { get; set; }
    }

    /// <summary>
    /// Contains item status statistics
    /// </summary>
    public class ItemStatusStatistics
    {
        public int TotalItemsCount { get; set; }
        public int AvailableCount { get; set; }
        public int BorrowedCount { get; set; }
        public int DamagedCount { get; set; }
        public int DestroyCount { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; }
    }

    /// <summary>
    /// Contains fine statistics
    /// </summary>
    public class FineStatistics
    {
        public decimal TotalFinesCollected { get; set; }
        public int LateReturnCount { get; set; }
        public decimal AverageFine { get; set; }
        public int CompletedBorrowsCount { get; set; }
        public decimal CurrentOverdueAmount { get; set; }
        public int TotalOverdueItems { get; set; }
    }

    /// <summary>
    /// Contains comprehensive library statistics
    /// </summary>
    public class LibraryStatistics
    {
        public int TotalBorrowers { get; set; }
        public int TotalItems { get; set; }
        public BorrowingStatistics BorrowingStats { get; set; }
        public ItemStatusStatistics ItemStatusStats { get; set; }
        public FineStatistics FineStats { get; set; }
    }
}
