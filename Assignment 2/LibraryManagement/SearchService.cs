using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service class for searching and filtering library items
    /// Provides reusable query methods for the web search interface
    /// Uses the static Item.AllItems collection shared with console app
    /// </summary>
    public static class SearchService
    {
        /// <summary>
        /// Searches all items with multiple filter criteria
        /// Supports filtering by name, library code, and/or item type
        /// </summary>
        /// <param name="searchName">Partial name match (case-insensitive, optional)</param>
        /// <param name="searchCode">Exact library code match (case-insensitive, optional)</param>
        /// <param name="searchType">Item type filter: "Book", "Music", "Toy" (optional)</param>
        /// <returns>List of matching items, sorted by name</returns>
        public static List<Item> SearchItems(string searchName = "", string searchCode = "", string searchType = "")
        {
            var filtered = Item.AllItems.ToList();

            // Filter by name (case-insensitive partial match)
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                filtered = filtered
                    .Where(i => i.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by library code (case-insensitive exact match)
            if (!string.IsNullOrWhiteSpace(searchCode))
            {
                filtered = filtered
                    .Where(i => i.LibraryCode.Equals(searchCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by type (Book, Music, Toy)
            if (!string.IsNullOrWhiteSpace(searchType))
            {
                filtered = filtered
                    .Where(i => GetItemType(i).Equals(searchType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sort by name for consistent display
            return filtered.OrderBy(i => i.Name).ToList();
        }

        /// <summary>
        /// Gets all unique item names for autocomplete suggestions
        /// </summary>
        /// <param name="searchTerm">Partial name to search for (minimum 2 characters)</param>
        /// <param name="maxResults">Maximum results to return (default: 10)</param>
        /// <returns>List of matching unique item names</returns>
        public static List<string> GetNameSuggestions(string searchTerm, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                return new List<string>();

            return Item.AllItems
                .Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(i => i.Name)
                .Distinct()
                .OrderBy(n => n)
                .Take(maxResults)
                .ToList();
        }

        /// <summary>
        /// Gets all unique library codes for autocomplete suggestions
        /// </summary>
        /// <param name="searchTerm">Partial code to search for (minimum 2 characters)</param>
        /// <param name="maxResults">Maximum results to return (default: 10)</param>
        /// <returns>List of matching unique library codes</returns>
        public static List<string> GetCodeSuggestions(string searchTerm, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                return new List<string>();

            return Item.AllItems
                .Where(i => i.LibraryCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(i => i.LibraryCode)
                .Distinct()
                .OrderBy(c => c)
                .Take(maxResults)
                .ToList();
        }

        /// <summary>
        /// Gets all available item types in the catalog
        /// </summary>
        /// <returns>List of unique item types (Book, Music, Toy)</returns>
        public static List<string> GetAvailableTypes()
        {
            return Item.AllItems
                .Select(i => GetItemType(i))
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        /// <summary>
        /// Gets count of items by status
        /// </summary>
        /// <returns>Dictionary with status as key and count as value</returns>
        public static Dictionary<string, int> GetItemCountByStatus()
        {
            return Item.AllItems
                .GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Gets count of items by type
        /// </summary>
        /// <returns>Dictionary with type as key and count as value</returns>
        public static Dictionary<string, int> GetItemCountByType()
        {
            return Item.AllItems
                .GroupBy(i => GetItemType(i))
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Gets items by their availability status
        /// </summary>
        /// <param name="status">Status filter (Available, Borrowed, Damaged, Destroy)</param>
        /// <returns>List of items with the specified status</returns>
        public static List<Item> GetItemsByStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return new List<Item>();

            return Item.AllItems
                .Where(i => i.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Name)
                .ToList();
        }

        /// <summary>
        /// Gets items of a specific type
        /// </summary>
        /// <param name="itemType">Type filter: "Book", "Music", or "Toy"</param>
        /// <returns>List of items of the specified type</returns>
        public static List<Item> GetItemsByType(string itemType)
        {
            if (string.IsNullOrWhiteSpace(itemType))
                return new List<Item>();

            return Item.AllItems
                .Where(i => GetItemType(i).Equals(itemType, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Name)
                .ToList();
        }

        /// <summary>
        /// Gets total count of items in the catalog
        /// </summary>
        /// <returns>Number of items</returns>
        public static int GetTotalItemCount()
        {
            return Item.AllItems.Count;
        }

        /// <summary>
        /// Gets count of available items (not borrowed, damaged, or destroyed)
        /// </summary>
        /// <returns>Number of available items</returns>
        public static int GetAvailableItemCount()
        {
            return Item.AllItems.Count(i => i.Status == "Available");
        }

        /// <summary>
        /// Gets the type name of an item (Book, Music, or Toy)
        /// </summary>
        private static string GetItemType(Item item)
        {
            if (item is Book) return "Book";
            if (item is Music) return "Music";
            if (item is Toy) return "Toy";
            return "Unknown";
        }

        /// <summary>
        /// Finds an item by its exact library code
        /// </summary>
        /// <param name="libraryCode">The library code to search for</param>
        /// <returns>The item if found, null otherwise</returns>
        public static Item FindByLibraryCode(string libraryCode)
        {
            if (string.IsNullOrWhiteSpace(libraryCode))
                return null;

            return Item.AllItems.FirstOrDefault(i =>
                i.LibraryCode.Equals(libraryCode, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Searches items by name (partial match)
        /// </summary>
        /// <param name="namePattern">Partial name to search for</param>
        /// <returns>List of matching items, sorted by name</returns>
        public static List<Item> SearchByName(string namePattern)
        {
            if (string.IsNullOrWhiteSpace(namePattern))
                return new List<Item>();

            return Item.AllItems
                .Where(i => i.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Name)
                .ToList();
        }

        /// <summary>
        /// Searches items by description
        /// </summary>
        /// <param name="descriptionPattern">Partial description text to search for</param>
        /// <returns>List of matching items, sorted by name</returns>
        public static List<Item> SearchByDescription(string descriptionPattern)
        {
            if (string.IsNullOrWhiteSpace(descriptionPattern))
                return new List<Item>();

            return Item.AllItems
                .Where(i => !string.IsNullOrWhiteSpace(i.Description) &&
                           i.Description.Contains(descriptionPattern, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Name)
                .ToList();
        }
    }
}
