using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service class for managing borrowers and checkout/return operations at the reception desk
    /// </summary>
    public class ReceptionService
    {
        /// <summary>
        /// Creates and adds a new borrower to the system
        /// </summary>
        public bool CreateBorrower(string borrowerId, string name, string phone, string email)
        {
            try
            {
                if (BorrowerIdExists(borrowerId))
                {
                    Console.WriteLine($"Error: BorrowerId '{borrowerId}' already exists.");
                    return false;
                }

                Borrower borrower = new Borrower(borrowerId, name, phone, email);
                Borrower.AllBorrowers.Add(borrower);
                Console.WriteLine($"✓ Borrower '{name}' created successfully with ID '{borrowerId}'.");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error creating borrower: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Finds a borrower by BorrowerId
        /// </summary>
        public Borrower FindBorrowerById(string borrowerId)
        {
            var borrower = Borrower.AllBorrowers.FirstOrDefault(b => b.BorrowerId == borrowerId);
            if (borrower == null)
            {
                Console.WriteLine($"No borrower found with ID '{borrowerId}'.");
            }
            return borrower;
        }

        /// <summary>
        /// Searches for borrowers by name (partial match)
        /// </summary>
        public List<Borrower> SearchBorrowerByName(string searchTerm)
        {
            var results = Borrower.AllBorrowers
                .Where(b => b.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (results.Count == 0)
            {
                Console.WriteLine($"No borrowers found matching '{searchTerm}'.");
            }

            return results;
        }

        /// <summary>
        /// Updates borrower information
        /// </summary>
        public bool UpdateBorrower(string borrowerId, string propertyName, string newValue)
        {
            var borrower = FindBorrowerById(borrowerId);
            if (borrower == null)
            {
                return false;
            }

            try
            {
                switch (propertyName.ToLower())
                {
                    case "name":
                        if (string.IsNullOrWhiteSpace(newValue))
                        {
                            Console.WriteLine("Error: Name cannot be empty.");
                            return false;
                        }
                        borrower.Name = newValue;
                        Console.WriteLine($"✓ Borrower name updated to '{newValue}'.");
                        return true;

                    case "phone":
                        borrower.Phone = newValue ?? "";
                        Console.WriteLine($"✓ Borrower phone updated to '{newValue}'.");
                        return true;

                    case "email":
                        borrower.Email = newValue ?? "";
                        Console.WriteLine($"✓ Borrower email updated to '{newValue}'.");
                        return true;

                    default:
                        Console.WriteLine($"Error: Unknown property '{propertyName}'.");
                        return false;
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error updating borrower: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a borrower by BorrowerId
        /// </summary>
        public bool DeleteBorrower(string borrowerId)
        {
            var borrower = FindBorrowerById(borrowerId);
            if (borrower == null)
            {
                return false;
            }

            // Check if borrower has active borrow records
            var activeBorrows = BorrowRecord.AllBorrowRecords
                .Where(br => br.BorrowerId == borrowerId && !br.ReturnDate.HasValue)
                .ToList();

            if (activeBorrows.Count > 0)
            {
                Console.WriteLine($"Error: Cannot delete borrower with {activeBorrows.Count} active borrow(s).");
                return false;
            }

            Borrower.AllBorrowers.Remove(borrower);
            Console.WriteLine($"✓ Borrower '{borrowerId}' deleted successfully.");
            return true;
        }

        /// <summary>
        /// Displays all borrowers
        /// </summary>
        public void DisplayAllBorrowers()
        {
            if (Borrower.AllBorrowers.Count == 0)
            {
                Console.WriteLine("No borrowers in the system.");
                return;
            }

            Console.WriteLine("\n--- All Borrowers ---");
            for (int i = 0; i < Borrower.AllBorrowers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Borrower.AllBorrowers[i]}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Borrows an item: creates a borrow record and updates item status to "Borrowed"
        /// </summary>
        public bool BorrowItem(string libraryCode, string borrowerId)
        {
            try
            {
                // Check if item exists
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == libraryCode);
                if (item == null)
                {
                    Console.WriteLine($"Error: Item with LibraryCode '{libraryCode}' not found.");
                    return false;
                }

                // Check if borrower exists
                if (FindBorrowerById(borrowerId) == null)
                {
                    return false;
                }

                // Check if item is already borrowed
                var activeBorrow = BorrowRecord.AllBorrowRecords
                    .FirstOrDefault(br => br.LibraryCode == libraryCode && !br.ReturnDate.HasValue);
                if (activeBorrow != null)
                {
                    Console.WriteLine($"Error: Item '{libraryCode}' is already borrowed by {activeBorrow.BorrowerId}.");
                    return false;
                }

                // Check if item is in a borrowable status
                if (item.Status != "Available")
                {
                    Console.WriteLine($"Error: Item '{libraryCode}' is not available (Status: {item.Status}).");
                    return false;
                }

                // Create borrow record
                BorrowRecord borrowRecord = new BorrowRecord(libraryCode, borrowerId);
                BorrowRecord.AllBorrowRecords.Add(borrowRecord);

                // Update item status to Borrowed
                item.SetStatus("Borrowed");

                Console.WriteLine($"✓ Item '{libraryCode}' borrowed successfully by {borrowerId}.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error borrowing item: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns an item: updates item status to "Available", sets return date, and calculates fine
        /// </summary>
        public bool ReturnItem(string libraryCode)
        {
            try
            {
                // Find the active borrow record
                var borrowRecord = BorrowRecord.AllBorrowRecords
                    .FirstOrDefault(br => br.LibraryCode == libraryCode && !br.ReturnDate.HasValue);

                if (borrowRecord == null)
                {
                    Console.WriteLine($"Error: No active borrow record found for '{libraryCode}'.");
                    return false;
                }

                // Set return date
                borrowRecord.ReturnDate = DateTime.Now;

                // Calculate fine
                borrowRecord.FineAmount = borrowRecord.CalculateFine();

                // Update item status to Available
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == libraryCode);
                if (item != null)
                {
                    item.SetStatus("Available");
                }

                Console.WriteLine($"✓ Item '{libraryCode}' returned successfully.");
                if (borrowRecord.FineAmount > 0)
                {
                    Console.WriteLine($"  Late fine: ${borrowRecord.FineAmount:F2} ({borrowRecord.GetDaysBorrowed()} days borrowed)");
                }
                else
                {
                    Console.WriteLine($"  No fine. Item returned within 14 days.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error returning item: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets all active borrow records
        /// </summary>
        public List<BorrowRecord> GetActiveBorrows()
        {
            return BorrowRecord.AllBorrowRecords
                .Where(br => !br.ReturnDate.HasValue)
                .ToList();
        }

        /// <summary>
        /// Gets all borrow records for a specific borrower
        /// </summary>
        public List<BorrowRecord> GetBorrowerHistory(string borrowerId)
        {
            var records = BorrowRecord.AllBorrowRecords
                .Where(br => br.BorrowerId == borrowerId)
                .ToList();

            if (records.Count == 0)
            {
                Console.WriteLine($"No borrow history found for borrower '{borrowerId}'.");
            }

            return records;
        }

        /// <summary>
        /// Displays all borrow records
        /// </summary>
        public void DisplayAllBorrowRecords()
        {
            if (BorrowRecord.AllBorrowRecords.Count == 0)
            {
                Console.WriteLine("No borrow records in the system.");
                return;
            }

            Console.WriteLine("\n--- All Borrow Records ---");
            for (int i = 0; i < BorrowRecord.AllBorrowRecords.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {BorrowRecord.AllBorrowRecords[i]}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Displays overdue items
        /// </summary>
        public void DisplayOverdueItems()
        {
            var overdueItems = BorrowRecord.AllBorrowRecords
                .Where(br => !br.ReturnDate.HasValue && br.IsOverdue())
                .ToList();

            if (overdueItems.Count == 0)
            {
                Console.WriteLine("No overdue items.");
                return;
            }

            Console.WriteLine("\n--- Overdue Items (>14 days) ---");
            for (int i = 0; i < overdueItems.Count; i++)
            {
                var record = overdueItems[i];
                Console.WriteLine($"{i + 1}. {record.LibraryCode} (Borrower: {record.BorrowerId}, " +
                    $"Days: {record.GetDaysBorrowed()}, Fine: ${record.CalculateFine():F2})");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Checks if a BorrowerId already exists
        /// </summary>
        private bool BorrowerIdExists(string borrowerId)
        {
            return Borrower.AllBorrowers.Any(b => b.BorrowerId == borrowerId);
        }
    }
}
