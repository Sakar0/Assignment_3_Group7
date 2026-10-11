using Assignment2.LibraryManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Public search controller for library item catalog
    /// Provides read-only search and browse functionality
    /// All data comes from static Item.AllItems collection shared with console app
    /// </summary>
    public class SearchController : Controller
    {
        /// <summary>
        /// Search page - displays search form and results
        /// Supports filtering by Name, LibraryCode, or Type (Book/Music/Toy)
        /// </summary>
        public IActionResult Index(string searchName = "", string searchCode = "", string searchType = "")
        {
            // Get all items from the shared static collection
            var allItems = Item.AllItems.ToList();

            // Apply filters
            var results = FilterItems(allItems, searchName, searchCode, searchType);

            // Store search criteria in ViewBag for display
            ViewBag.SearchName = searchName;
            ViewBag.SearchCode = searchCode;
            ViewBag.SearchType = searchType;
            ViewBag.ResultCount = results.Count;

            return View(results);
        }

        /// <summary>
        /// Filters items based on search criteria
        /// Returns all items if no filters provided
        /// </summary>
        private List<Item> FilterItems(List<Item> items, string searchName, string searchCode, string searchType)
        {
            var filtered = items;

            // Filter by name (case-insensitive partial match)
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                filtered = filtered
                    .Where(i => i.Name.Contains(searchName, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by library code (case-insensitive exact match)
            if (!string.IsNullOrWhiteSpace(searchCode))
            {
                filtered = filtered
                    .Where(i => i.LibraryCode.Equals(searchCode, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by type (Book, Music, Toy)
            if (!string.IsNullOrWhiteSpace(searchType))
            {
                filtered = filtered
                    .Where(i => GetItemType(i).Equals(searchType, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sort by name for consistent display
            return filtered.OrderBy(i => i.Name).ToList();
        }

        /// <summary>
        /// Gets the type name of an item (Book, Music, or Toy)
        /// </summary>
        private string GetItemType(Item item)
        {
            if (item is Book) return "Book";
            if (item is Music) return "Music";
            if (item is Toy) return "Toy";
            return "Unknown";
        }

        /// <summary>
        /// API endpoint for autocomplete/live search suggestions
        /// Returns JSON array of matching item names
        /// </summary>
        [HttpGet]
        public IActionResult GetNameSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<string>());

            var suggestions = Item.AllItems
                .Where(i => i.Name.Contains(term, System.StringComparison.OrdinalIgnoreCase))
                .Select(i => i.Name)
                .Distinct()
                .OrderBy(n => n)
                .Take(10)
                .ToList();

            return Json(suggestions);
        }

        /// <summary>
        /// API endpoint for library code autocomplete
        /// Returns JSON array of matching library codes
        /// </summary>
        [HttpGet]
        public IActionResult GetCodeSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<string>());

            var suggestions = Item.AllItems
                .Where(i => i.LibraryCode.Contains(term, System.StringComparison.OrdinalIgnoreCase))
                .Select(i => i.LibraryCode)
                .Distinct()
                .OrderBy(c => c)
                .Take(10)
                .ToList();

            return Json(suggestions);
        }

        /// <summary>
        /// API endpoint to get items by type
        /// Used for the Type filter dropdown
        /// </summary>
        [HttpGet]
        public IActionResult GetAvailableTypes()
        {
            var types = Item.AllItems
                .Select(i => GetItemType(i))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return Json(types);
        }

        /// <summary>
        /// Return to home page or search
        /// </summary>
        public IActionResult ClearFilters()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
