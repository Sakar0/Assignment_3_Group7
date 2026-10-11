using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// BorrowRecord class - tracks when someone borrows an item and when they return it
    /// Also calculates if the item is overdue and how much fine is owed
    /// </summary>
    public class BorrowRecord
    {
        public string LibraryCode { get; set; }   // Which item was borrowed (like "BOOK001")
        public string BorrowerId { get; set; }    // Who borrowed it (like "STU001")
        public DateTime BorrowDate { get; set; }  // When it was borrowed (set to now)
        public DateTime? ReturnDate { get; set; } // When it was returned (null if still borrowed)
        public decimal FineAmount { get; set; }   // How much money is owed for being late

        // Shared list - stores ALL borrow transactions in the system
        public static List<BorrowRecord> AllBorrowRecords { get; } = new List<BorrowRecord>();

        /// <summary>
        /// Create a new borrow record
        /// This is created when someone checks out an item
        /// LibraryCode and BorrowerId are required
        /// BorrowDate is automatically set to today
        /// ReturnDate starts as null (will be set when item is returned)
        /// </summary>
        public BorrowRecord(string libraryCode, string borrowerId)
        {
            // Make sure LibraryCode is not empty
            if (string.IsNullOrWhiteSpace(libraryCode))
                throw new ArgumentException("LibraryCode cannot be empty.");

            // Make sure BorrowerId is not empty
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            LibraryCode = libraryCode;
            BorrowerId = borrowerId;
            BorrowDate = DateTime.Now;  // Set to current date/time
            ReturnDate = null;          // Not returned yet
            FineAmount = 0;             // No fine yet
        }

        /// <summary>
        /// How many days has the item been borrowed?
        /// If still borrowed: days from borrow date to today
        /// If returned: days from borrow date to return date
        /// </summary>
        public int GetDaysBorrowed()
        {
            // Use return date if item was returned, otherwise use today's date
            DateTime endDate = ReturnDate ?? DateTime.Now;
            // Calculate days between dates
            return (int)(endDate - BorrowDate).TotalDays;
        }

        /// <summary>
        /// Is the item overdue?
        /// Overdue = borrowed for more than 14 days
        /// </summary>
        public bool IsOverdue()
        {
            return GetDaysBorrowed() > 14;
        }

        /// <summary>
        /// Calculate the fine amount
        /// $1 for each day over the 14-day limit
        /// Example: 20-day borrow = (20 - 14) × $1 = $6
        /// </summary>
        public decimal CalculateFine()
        {
            int daysBorrowed = GetDaysBorrowed();
            // If more than 14 days, charge $1 per extra day
            if (daysBorrowed > 14)
            {
                return (daysBorrowed - 14) * 1.0m;
            }
            // If 14 days or less, no fine
            return 0;
        }

        /// <summary>
        /// Display borrow record information as text
        /// </summary>
        public override string ToString()
        {
            // Show "Returned: date" if item was returned, otherwise "Still Borrowed"
            string returnInfo = ReturnDate.HasValue 
                ? $"Returned: {ReturnDate:yyyy-MM-dd HH:mm}" 
                : "Still Borrowed";

            return $"LibraryCode: {LibraryCode}, BorrowerId: {BorrowerId}, " + 
                   $"BorrowDate: {BorrowDate:yyyy-MM-dd HH:mm}, {returnInfo}, Fine: ${FineAmount:F2}";
        }
    }
}
