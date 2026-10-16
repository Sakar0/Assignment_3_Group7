using Assignment2.LibraryManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.Controllers
{
    public class WaitlistController : Controller
    {
        /// <summary>
        /// GET: Waitlist/Index
        /// Displays waitlist entries for the selected borrower
        /// </summary>
        public IActionResult Index()
        {
            var borrowers = Borrower.AllBorrowers;
            ViewBag.Borrowers = borrowers;

            string? selectedBorrowerId = Request.Query["borrowerId"];
            List<WaitlistEntry> waitlistEntries = new List<WaitlistEntry>();

            if (!string.IsNullOrWhiteSpace(selectedBorrowerId))
            {
                ViewBag.SelectedBorrowerId = selectedBorrowerId;
                waitlistEntries = WaitlistService.GetWaitlistEntriesByBorrower(selectedBorrowerId);
            }

            return View(waitlistEntries);
        }

        /// <summary>
        /// GET: Waitlist/Join
        /// Shows form to join the waitlist
        /// </summary>
        public IActionResult Join()
        {
            var borrowers = Borrower.AllBorrowers;
            ViewBag.Borrowers = borrowers;

            // Get all unavailable items for waitlist
            var unavailableItems = Item.AllItems
                .Where(i => i.Status != "Available")
                .OrderBy(i => i.Name)
                .ToList();

            return View(unavailableItems);
        }

        /// <summary>
        /// POST: Waitlist/Join
        /// Adds a borrower to the waitlist for an item
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Join(string borrowerId, string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(borrowerId) || string.IsNullOrWhiteSpace(itemLibraryCode))
            {
                TempData["Error"] = "Borrower ID and Item code are required.";
                return RedirectToAction("Join");
            }

            try
            {
                WaitlistService.AddToWaitlist(borrowerId, itemLibraryCode);
                TempData["Success"] = "Successfully added to waitlist. You will be notified when the item is available.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
            }

            return RedirectToAction("Index", new { borrowerId });
        }

        /// <summary>
        /// GET: Waitlist/Details/id
        /// Shows details of a specific waitlist entry
        /// </summary>
        public IActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var entry = WaitlistService.GetWaitlistEntryById(id);
            if (entry == null)
                return NotFound();

            // Get the item details
            var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == entry.ItemLibraryCode);
            ViewBag.Item = item;

            return View(entry);
        }

        /// <summary>
        /// GET: Waitlist/MyOffers
        /// Shows all pending offers for the current borrower (requires borrowerId submission)
        /// </summary>
        public IActionResult MyOffers()
        {
            var borrowers = Borrower.AllBorrowers;
            ViewBag.Borrowers = borrowers;

            string? selectedBorrowerId = Request.Query["borrowerId"];
            List<WaitlistEntry> offers = new List<WaitlistEntry>();

            if (!string.IsNullOrWhiteSpace(selectedBorrowerId))
            {
                ViewBag.SelectedBorrowerId = selectedBorrowerId;
                // Get all OfferMade entries for this borrower
                offers = WaitlistService.GetWaitlistEntriesByBorrower(selectedBorrowerId)
                    .Where(w => w.Status == "OfferMade" && !w.HasOfferExpired())
                    .ToList();
            }

            return View(offers);
        }

        /// <summary>
        /// POST: Waitlist/AcceptOffer
        /// Borrower accepts an offer
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptOffer(string waitlistEntryId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                TempData["Error"] = "Missing required information.";
                return RedirectToAction("MyOffers");
            }

            try
            {
                WaitlistService.AcceptOffer(waitlistEntryId, borrowerId);
                TempData["Success"] = "Offer accepted! Please go to the circulation desk to complete the borrowing process.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
            }

            return RedirectToAction("MyOffers", new { borrowerId });
        }

        /// <summary>
        /// POST: Waitlist/DeclineOffer
        /// Borrower declines an offer
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeclineOffer(string waitlistEntryId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                TempData["Error"] = "Missing required information.";
                return RedirectToAction("MyOffers");
            }

            try
            {
                WaitlistService.DeclineOffer(waitlistEntryId, borrowerId);
                TempData["Success"] = "Offer declined. The next person in queue will be offered the item.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
            }

            return RedirectToAction("MyOffers", new { borrowerId });
        }

        /// <summary>
        /// POST: Waitlist/Cancel
        /// Borrower cancels their waitlist entry
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(string waitlistEntryId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                TempData["Error"] = "Missing required information.";
                return RedirectToAction("Index");
            }

            try
            {
                WaitlistService.CancelWaitlistEntry(waitlistEntryId, borrowerId);
                TempData["Success"] = "Waitlist entry cancelled successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
            }

            return RedirectToAction("Index", new { borrowerId });
        }
    }
}
