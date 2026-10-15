using System;
using System.Collections.Generic;
using System.Text;
using Assignment2.LibraryManagement;

namespace Assignment2.Services
{
    /// <summary>
    /// Service for exporting manager reports to CSV and PDF formats
    /// </summary>
    public class ReportExportService
    {
        private readonly ManagerService _managerService = new ManagerService();

        /// <summary>
        /// Generates a CSV export of the comprehensive library statistics
        /// </summary>
        public string GenerateCsvReport()
        {
            var stats = _managerService.GetComprehensiveStatistics();
            var csv = new StringBuilder();

            // Report header
            csv.AppendLine("Library Management System - Analytics Report");
            csv.AppendLine($"Generated: {DateTime.Now:MMMM dd, yyyy 'at' HH:mm:ss}");
            csv.AppendLine();

            // Overall Statistics
            csv.AppendLine("=== OVERALL STATISTICS ===");
            csv.AppendLine($"Total Items,{stats.TotalItems}");
            csv.AppendLine($"Total Borrowers,{stats.TotalBorrowers}");
            csv.AppendLine();

            // Item Status Statistics
            csv.AppendLine("=== ITEM STATUS BREAKDOWN ===");
            csv.AppendLine("Status,Count,Percentage");
            csv.AppendLine($"Available,{stats.ItemStatusStats.AvailableCount},\"{GetPercentage(stats.ItemStatusStats.AvailableCount, stats.TotalItems)}%\"");
            csv.AppendLine($"Borrowed,{stats.ItemStatusStats.BorrowedCount},\"{GetPercentage(stats.ItemStatusStats.BorrowedCount, stats.TotalItems)}%\"");
            csv.AppendLine($"Damaged,{stats.ItemStatusStats.DamagedCount},\"{GetPercentage(stats.ItemStatusStats.DamagedCount, stats.TotalItems)}%\"");
            csv.AppendLine($"Marked for Destroy,{stats.ItemStatusStats.DestroyCount},\"{GetPercentage(stats.ItemStatusStats.DestroyCount, stats.TotalItems)}%\"");
            csv.AppendLine();

            // Borrowing Statistics
            csv.AppendLine("=== BORROWING ACTIVITY ===");
            csv.AppendLine($"Currently Borrowed Items,{stats.BorrowingStats.CurrentlyBorrowedCount}");
            csv.AppendLine($"Total Borrow Transactions,{stats.BorrowingStats.TotalBorrowsCount}");
            csv.AppendLine();

            // Most Borrowed Items
            if (stats.BorrowingStats.MostBorrowedItems.Count > 0)
            {
                csv.AppendLine("=== TOP BORROWED ITEMS ===");
                csv.AppendLine("Rank,Item Name,Library Code,Times Borrowed");
                for (int i = 0; i < stats.BorrowingStats.MostBorrowedItems.Count; i++)
                {
                    var item = stats.BorrowingStats.MostBorrowedItems[i];
                    csv.AppendLine($"\"{i + 1}\",\"{EscapeCsv(item.ItemName)}\",\"{item.LibraryCode}\",{item.BorrowCount}");
                }
                csv.AppendLine();
            }

            // Fine Statistics
            csv.AppendLine("=== FINE & OVERDUE MANAGEMENT ===");
            csv.AppendLine($"Total Fines Collected,${stats.FineStats.TotalFinesCollected:F2}");
            csv.AppendLine($"Late Returns,{stats.FineStats.LateReturnCount}");
            csv.AppendLine($"Average Fine per Item,${stats.FineStats.AverageFine:F2}");
            csv.AppendLine($"Completed Transactions,{stats.FineStats.CompletedBorrowsCount}");
            csv.AppendLine($"Late Return Rate,\"{GetPercentage(stats.FineStats.LateReturnCount, stats.FineStats.CompletedBorrowsCount)}%\"");
            csv.AppendLine($"Overdue Items,{stats.FineStats.TotalOverdueItems}");
            csv.AppendLine($"Current Overdue Amount,${stats.FineStats.CurrentOverdueAmount:F2}");
            csv.AppendLine();

            return csv.ToString();
        }

        /// <summary>
        /// Generates a CSV export with item type breakdown
        /// </summary>
        public string GenerateItemBreakdownCsv()
        {
            var csv = new StringBuilder();

            // Report header
            csv.AppendLine("Library Management System - Item Type Breakdown");
            csv.AppendLine($"Generated: {DateTime.Now:MMMM dd, yyyy 'at' HH:mm:ss}");
            csv.AppendLine();

            // Books by Genre
            csv.AppendLine("=== BOOKS BY GENRE ===");
            csv.AppendLine("Genre,Count");
            var booksByGenre = GetBooksByGenre();
            foreach (var genre in booksByGenre.Keys)
            {
                csv.AppendLine($"\"{EscapeCsv(genre)}\",{booksByGenre[genre]}");
            }
            csv.AppendLine();

            // Music by Artist
            csv.AppendLine("=== MUSIC BY ARTIST ===");
            csv.AppendLine("Artist,Count");
            var musicByArtist = GetMusicByArtist();
            foreach (var artist in musicByArtist.Keys)
            {
                csv.AppendLine($"\"{EscapeCsv(artist)}\",{musicByArtist[artist]}");
            }
            csv.AppendLine();

            // Toys by Age Group
            csv.AppendLine("=== TOYS BY AGE GROUP ===");
            csv.AppendLine("Age Group,Count");
            var toysByAgeGroup = GetToysByAgeGroup();
            foreach (var ageGroup in toysByAgeGroup.Keys)
            {
                csv.AppendLine($"\"{EscapeCsv(ageGroup)}\",{toysByAgeGroup[ageGroup]}");
            }
            csv.AppendLine();

            return csv.ToString();
        }

        /// <summary>
        /// Generates a text report suitable for display or PDF conversion
        /// </summary>
        public string GenerateTextReport()
        {
            var stats = _managerService.GetComprehensiveStatistics();
            var report = new StringBuilder();

            report.AppendLine("╔═══════════════════════════════════════════════════════╗");
            report.AppendLine("║   LIBRARY MANAGEMENT SYSTEM - ANALYTICS REPORT       ║");
            report.AppendLine("╚═══════════════════════════════════════════════════════╝");
            report.AppendLine();
            report.AppendLine($"Generated: {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}");
            report.AppendLine();

            // Overall Statistics
            report.AppendLine("┌─ Overall Statistics ─────────────────────────────────┐");
            report.AppendLine($"│ Total Items in Library:           {stats.TotalItems,10} │");
            report.AppendLine($"│ Total Registered Borrowers:       {stats.TotalBorrowers,10} │");
            report.AppendLine($"│ Currently Borrowed Items:         {stats.BorrowingStats.CurrentlyBorrowedCount,10} │");
            report.AppendLine($"│ Total Borrow Transactions:        {stats.BorrowingStats.TotalBorrowsCount,10} │");
            report.AppendLine("└───────────────────────────────────────────────────────┘");
            report.AppendLine();

            // Item Status
            report.AppendLine("┌─ Item Status ─────────────────────────────────────────┐");
            report.AppendLine($"│ Available:         {stats.ItemStatusStats.AvailableCount,5} items ({GetPercentage(stats.ItemStatusStats.AvailableCount, stats.TotalItems),5}%) │");
            report.AppendLine($"│ Borrowed:          {stats.ItemStatusStats.BorrowedCount,5} items ({GetPercentage(stats.ItemStatusStats.BorrowedCount, stats.TotalItems),5}%) │");
            report.AppendLine($"│ Damaged:           {stats.ItemStatusStats.DamagedCount,5} items ({GetPercentage(stats.ItemStatusStats.DamagedCount, stats.TotalItems),5}%) │");
            report.AppendLine($"│ Marked for Destroy:{stats.ItemStatusStats.DestroyCount,5} items ({GetPercentage(stats.ItemStatusStats.DestroyCount, stats.TotalItems),5}%) │");
            report.AppendLine("└───────────────────────────────────────────────────────┘");
            report.AppendLine();

            // Fine Statistics
            report.AppendLine("┌─ Fine & Overdue Management ──────────────────────────┐");
            report.AppendLine($"│ Total Fines Collected:            ${stats.FineStats.TotalFinesCollected,9:F2} │");
            report.AppendLine($"│ Average Fine per Item:            ${stats.FineStats.AverageFine,9:F2} │");
            report.AppendLine($"│ Late Returns:                     {stats.FineStats.LateReturnCount,10} │");
            report.AppendLine($"│ Overdue Items (Current):          {stats.FineStats.TotalOverdueItems,10} │");
            report.AppendLine($"│ Current Overdue Amount:           ${stats.FineStats.CurrentOverdueAmount,9:F2} │");
            report.AppendLine("└───────────────────────────────────────────────────────┘");
            report.AppendLine();

            return report.ToString();
        }

        /// <summary>
        /// Helper method to escape CSV values containing commas, quotes, or newlines
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // If value contains comma, quote, or newline, wrap in quotes and escape internal quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
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
        /// Gets books grouped by genre
        /// </summary>
        private Dictionary<string, int> GetBooksByGenre()
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in Item.AllItems)
            {
                if (item is Book book)
                {
                    if (!result.ContainsKey(book.Genre))
                        result[book.Genre] = 0;
                    result[book.Genre]++;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets music grouped by artist
        /// </summary>
        private Dictionary<string, int> GetMusicByArtist()
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in Item.AllItems)
            {
                if (item is Music music)
                {
                    if (!result.ContainsKey(music.Artist))
                        result[music.Artist] = 0;
                    result[music.Artist]++;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets toys grouped by age group
        /// </summary>
        private Dictionary<string, int> GetToysByAgeGroup()
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in Item.AllItems)
            {
                if (item is Toy toy)
                {
                    if (!result.ContainsKey(toy.AgeGroup))
                        result[toy.AgeGroup] = 0;
                    result[toy.AgeGroup]++;
                }
            }
            return result;
        }
    }
}
