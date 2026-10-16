using Assignment2.LibraryManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Self-service library kiosk.
    /// Provides a touch-friendly landing page plus catalogue browsing/search and
    /// item detail/availability views. Checkout and return operations reuse the
    /// existing ReceptionService business rules (Stages 5-6 wire the actions).
    /// Read-only browsing requires no identification; borrowing identifies the
    /// user by their BorrowerId (the system's existing identity mechanism).
    /// </summary>
    public class KioskController : Controller
    {
        // Service instance is cheap to create (works on shared static data).
        private readonly ReceptionService _receptionService = new ReceptionService();

        /// <summary>
        /// GET /Kiosk - kiosk landing page with the main self-service actions.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET /Kiosk/Catalogue - browse/search the catalogue.
        /// Reuses SearchService.SearchItems; results are sorted by name.
        /// </summary>
        [HttpGet]
        public IActionResult Catalogue(string searchName = "", string searchCode = "", string searchType = "")
        {
            var results = SearchService.SearchItems(searchName, searchCode, searchType);

            ViewBag.SearchName = searchName;
            ViewBag.SearchCode = searchCode;
            ViewBag.SearchType = searchType;
            ViewBag.ResultCount = results.Count;
            ViewBag.Types = SearchService.GetAvailableTypes();

            return View(results);
        }

        /// <summary>
        /// GET /Kiosk/Item/{code} - item details and availability.
        /// </summary>
        [HttpGet("/Kiosk/Item/{code}")]
        public IActionResult Item(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "No item code was provided.";
                return RedirectToAction(nameof(Catalogue));
            }

            var item = SearchService.FindByLibraryCode(code.Trim());
            if (item == null)
            {
                TempData["Error"] = $"No item with library code '{code.Trim()}' was found.";
                return RedirectToAction(nameof(Catalogue));
            }

            return View(item);
        }

        /// <summary>
        /// GET /Kiosk/Checkout/{code} - checkout confirmation form for an item.
        /// The user identifies themselves with their own BorrowerId.
        /// </summary>
        [HttpGet("/Kiosk/Checkout/{code}")]
        public IActionResult Checkout(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "No item code was provided.";
                return RedirectToAction(nameof(Catalogue));
            }

            var item = SearchService.FindByLibraryCode(code.Trim());
            if (item == null)
            {
                TempData["Error"] = $"No item with library code '{code.Trim()}' was found.";
                return RedirectToAction(nameof(Catalogue));
            }

            if (item.Status != "Available")
            {
                TempData["Error"] = $"'{item.Name}' is currently {item.Status.ToLower()} and cannot be checked out.";
                return RedirectToAction(nameof(Item), new { code = item.LibraryCode });
            }

            return View(item);
        }

        /// <summary>
        /// POST /Kiosk/Checkout - performs the checkout via the existing
        /// ReceptionService business rules (item exists, borrower exists,
        /// item available, no duplicate active borrow). Never bypasses rules.
        /// </summary>
        [HttpPost("/Kiosk/Checkout")]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string code, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "No item code was provided.";
                return RedirectToAction(nameof(Catalogue));
            }

            if (string.IsNullOrWhiteSpace(borrowerId))
            {
                TempData["Error"] = "Please enter your Borrower ID to check out an item.";
                return RedirectToAction(nameof(Checkout), new { code = code.Trim() });
            }

            code = code.Trim();
            borrowerId = borrowerId.Trim();

            // Re-validate item and availability immediately before borrowing
            // (prevents duplicate/conflicting checkouts if state changed).
            var item = SearchService.FindByLibraryCode(code);
            if (item == null)
            {
                TempData["Error"] = $"No item with library code '{code}' was found.";
                return RedirectToAction(nameof(Catalogue));
            }

            if (item.Status != "Available")
            {
                TempData["Error"] = $"'{item.Name}' is no longer available (Status: {item.Status}).";
                return RedirectToAction(nameof(Item), new { code });
            }

            // Delegate to the existing business logic - enforces all rules.
            bool success = _receptionService.BorrowItem(code, borrowerId);
            if (success)
            {
                DataService.SaveAllData();
                TempData["Success"] = $"'{item.Name}' ({code}) has been checked out to {borrowerId}. Enjoy!";
                return RedirectToAction(nameof(Index));
            }

            // BorrowItem already logs the specific reason; surface a safe message.
            TempData["Error"] = "Checkout failed. Please check that your Borrower ID is valid and the item is available.";
            return RedirectToAction(nameof(Checkout), new { code });
        }

        /// <summary>
        /// GET /Kiosk/Return - return form (item identified by library code).
        /// </summary>
        [HttpGet]
        public IActionResult Return()
        {
            return View();
        }

        /// <summary>
        /// POST /Kiosk/Return - returns an item via the existing
        /// ReceptionService business rules (sets return date, calculates any
        /// fine, restores availability).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Return(string libraryCode)
        {
            if (string.IsNullOrWhiteSpace(libraryCode))
            {
                TempData["Error"] = "Please enter the library code of the item you are returning.";
                return View();
            }

            libraryCode = libraryCode.Trim();

            var item = SearchService.FindByLibraryCode(libraryCode);
            if (item == null)
            {
                TempData["Error"] = $"No item with library code '{libraryCode}' was found.";
                return View();
            }

            bool success = _receptionService.ReturnItem(libraryCode);
            if (success)
            {
                DataService.SaveAllData();
                TempData["Success"] = $"Thank you! '{item.Name}' ({libraryCode}) has been returned.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = $"Return failed. '{item.Name}' does not appear to be currently borrowed.";
            return View();
        }

        /// <summary>
        /// GET /Kiosk/MyLoans - looks up a borrower's active loans by BorrowerId.
        /// Only the borrower's own records are shown (self-service).
        /// </summary>
        [HttpGet]
        public IActionResult MyLoans(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
            {
                return View((borrowerId, loans: null as List<BorrowRecord>));
            }

            borrowerId = borrowerId.Trim();

            var borrower = _receptionService.FindBorrowerById(borrowerId);
            if (borrower == null)
            {
                ViewBag.Error = $"No borrower found with ID '{borrowerId}'.";
                return View((borrowerId, loans: null as List<BorrowRecord>));
            }

            var active = _receptionService.GetBorrowerHistory(borrowerId)
                .Where(r => !r.ReturnDate.HasValue)
                .ToList();

            ViewBag.BorrowerName = borrower.Name;
            return View((borrowerId, loans: (List<BorrowRecord>?)active));
        }
    }
}

