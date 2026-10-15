using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service class for managing library branches
    /// Provides CRUD operations for branches and branch-related queries
    /// </summary>
    public class BranchService
    {
        /// <summary>
        /// Creates and adds a new library branch
        /// </summary>
        /// <param name="branchId">Unique branch identifier</param>
        /// <param name="branchName">Display name of the branch</param>
        /// <param name="location">Physical location/address</param>
        /// <returns>True if branch created successfully, false otherwise</returns>
        public bool CreateBranch(string branchId, string branchName, string location)
        {
            try
            {
                if (BranchIdExists(branchId))
                {
                    Console.WriteLine($"Error: BranchId '{branchId}' already exists.");
                    return false;
                }

                LibraryBranch branch = new LibraryBranch(branchId, branchName, location, isActive: true);
                LibraryBranch.AllBranches.Add(branch);
                Console.WriteLine($"✓ Branch '{branchName}' created successfully with ID '{branchId}'.");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error creating branch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Finds a branch by its BranchId
        /// </summary>
        public LibraryBranch FindBranchById(string branchId)
        {
            var branch = LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == branchId);
            if (branch == null)
            {
                Console.WriteLine($"No branch found with ID '{branchId}'.");
            }
            return branch;
        }

        /// <summary>
        /// Gets all active branches
        /// </summary>
        public List<LibraryBranch> GetActiveBranches()
        {
            return LibraryBranch.AllBranches
                .Where(b => b.IsActive)
                .ToList();
        }

        /// <summary>
        /// Gets all branches (both active and inactive)
        /// </summary>
        public List<LibraryBranch> GetAllBranches()
        {
            return new List<LibraryBranch>(LibraryBranch.AllBranches);
        }

        /// <summary>
        /// Updates branch information
        /// </summary>
        public bool UpdateBranch(string branchId, string propertyName, string newValue)
        {
            var branch = FindBranchById(branchId);
            if (branch == null)
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
                            Console.WriteLine("Error: Branch name cannot be empty.");
                            return false;
                        }
                        branch.BranchName = newValue;
                        Console.WriteLine($"✓ Branch name updated to '{newValue}'.");
                        return true;

                    case "location":
                        if (string.IsNullOrWhiteSpace(newValue))
                        {
                            Console.WriteLine("Error: Branch location cannot be empty.");
                            return false;
                        }
                        branch.Location = newValue;
                        Console.WriteLine($"✓ Branch location updated to '{newValue}'.");
                        return true;

                    case "active":
                        if (bool.TryParse(newValue, out bool isActive))
                        {
                            branch.IsActive = isActive;
                            string status = isActive ? "Active" : "Inactive";
                            Console.WriteLine($"✓ Branch status set to {status}.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Error: Active value must be 'true' or 'false'.");
                            return false;
                        }

                    default:
                        Console.WriteLine($"Error: Unknown property '{propertyName}'.");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating branch property: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deactivates a branch (soft delete - retains historical data)
        /// </summary>
        public bool DeactivateBranch(string branchId)
        {
            var branch = FindBranchById(branchId);
            if (branch == null)
            {
                return false;
            }

            branch.IsActive = false;
            Console.WriteLine($"✓ Branch '{branchId}' has been deactivated.");
            return true;
        }

        /// <summary>
        /// Reactivates a previously deactivated branch
        /// </summary>
        public bool ReactivateBranch(string branchId)
        {
            var branch = FindBranchById(branchId);
            if (branch == null)
            {
                return false;
            }

            branch.IsActive = true;
            Console.WriteLine($"✓ Branch '{branchId}' has been reactivated.");
            return true;
        }

        /// <summary>
        /// Displays all branches with their details
        /// </summary>
        public void DisplayAllBranches()
        {
            if (LibraryBranch.AllBranches.Count == 0)
            {
                Console.WriteLine("No branches in the system.");
                return;
            }

            Console.WriteLine("\n=== All Branches ===");
            foreach (var branch in LibraryBranch.AllBranches)
            {
                Console.WriteLine($"  {branch}");
            }
        }

        /// <summary>
        /// Counts items in a specific branch
        /// </summary>
        public int GetItemCountForBranch(string branchId)
        {
            return Item.AllItems.Count(i => i.BranchId == branchId);
        }

        /// <summary>
        /// Gets all items belonging to a specific branch
        /// </summary>
        public List<Item> GetItemsForBranch(string branchId)
        {
            return Item.AllItems
                .Where(i => i.BranchId == branchId)
                .ToList();
        }

        /// <summary>
        /// Check if a branch ID already exists
        /// </summary>
        private bool BranchIdExists(string branchId)
        {
            return LibraryBranch.AllBranches.Any(b => b.BranchId == branchId);
        }
    }
}
