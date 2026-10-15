using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// LibraryBranch model - represents a physical branch of the library system
    /// Each branch has a unique ID, name, location, and active status
    /// Serves as the foundation for multi-branch inventory and transfer features
    /// </summary>
    public class LibraryBranch
    {
        /// <summary>
        /// Unique identifier for the branch (e.g., "BR001", "BR002")
        /// Used to organize items, borrowers, and transfers by branch
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// Display name of the branch (e.g., "Downtown Library", "Campus Branch")
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Physical location/address of the branch
        /// Can include street address, phone, opening hours, etc.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Indicates whether this branch is currently active
        /// Inactive branches cannot process new transactions but retain historical data
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Timestamp when the branch was created
        /// Used for audit trail and branch history
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Static collection of all branches in the system
        /// Mirrors the pattern used for Item, Borrower, and BorrowRecord
        /// Enables consistent data model across the application
        /// </summary>
        public static List<LibraryBranch> AllBranches { get; } = new List<LibraryBranch>();

        /// <summary>
        /// Constructor for creating a new library branch
        /// </summary>
        /// <param name="branchId">Unique branch identifier</param>
        /// <param name="branchName">Display name of the branch</param>
        /// <param name="location">Physical location/address</param>
        /// <param name="isActive">Whether the branch is active</param>
        public LibraryBranch(string branchId, string branchName, string location, bool isActive = true)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(branchId))
                throw new ArgumentException("BranchId cannot be empty.");

            if (string.IsNullOrWhiteSpace(branchName))
                throw new ArgumentException("BranchName cannot be empty.");

            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location cannot be empty.");

            BranchId = branchId;
            BranchName = branchName;
            Location = location;
            IsActive = isActive;
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Parameterless constructor for JSON deserialization
        /// </summary>
        public LibraryBranch()
        {
            BranchId = string.Empty;
            BranchName = string.Empty;
            Location = string.Empty;
            IsActive = true;
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Display branch information as formatted text
        /// </summary>
        public override string ToString()
        {
            string status = IsActive ? "Active" : "Inactive";
            return $"BranchId: {BranchId}, Name: {BranchName}, Location: {Location}, Status: {status}";
        }
    }
}
