using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Base class for all library items (Books, Music, Toys)
    /// Contains common properties and behavior for any item in the library
    /// </summary>
    public abstract class Item
    {
        // Basic information about the item
        public string Name { get; set; }           // Name of the item
        public string Description { get; set; }     // What the item is about
        public string LibraryCode { get; set; }    // Unique identifier (like BOOK001)
        public string Status { get; set; }         // Available, Borrowed, Damaged, or Destroy

        // Branch information - identifies which branch owns this item
        public string BranchId { get; set; }       // Which branch this item belongs to (e.g., "BR001")

        // Shared list - stores ALL items from the entire library (Books + Music + Toys)
        public static List<Item> AllItems { get; } = new List<Item>();

        /// <summary>
        /// Constructor - called by derived classes (Book, Music, Toy)
        /// </summary>
        protected Item(string name, string description, string libraryCode, string status, string branchId = "DEFAULT")
        {
            Name = name;
            Description = description;
            LibraryCode = libraryCode;
            BranchId = branchId ?? "DEFAULT";

            // Check that status is one of the valid options
            ValidateStatus(status);
            Status = status;
        }

        /// <summary>
        /// Check if the status value is allowed
        /// Valid statuses: Available, Borrowed, Damaged, Destroy
        /// Throws an error if status is invalid
        /// </summary>
        protected void ValidateStatus(string status)
        {
            // List of allowed status values
            string[] allowedStatuses = { "Available", "Borrowed", "Damaged", "Destroy" };

            // Check if the status exists in the allowed list
            if (!Array.Exists(allowedStatuses, element => element == status))
            {
                // If not allowed, show an error message
                throw new ArgumentException($"Status must be one of: Available, Borrowed, Damaged, Destroy. Got: {status}");
            }
        }

        /// <summary>
        /// Change the item status to a new value
        /// Example: SetStatus("Borrowed") marks item as borrowed
        /// </summary>
        public void SetStatus(string newStatus)
        {
            // Verify the new status is valid before changing it
            ValidateStatus(newStatus);
            Status = newStatus;
        }

        /// <summary>
        /// Display the item information as text
        /// </summary>
        public override string ToString()
        {
            return $"Name: {Name}, Description: {Description}, LibraryCode: {LibraryCode}, Status: {Status}, BranchId: {BranchId}";
        }
    }
}
