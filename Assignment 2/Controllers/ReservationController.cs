using Assignment2.LibraryManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Controller for managing user reservations
    /// Handles viewing, creating, and cancelling reservations
    /// </summary>
    public class ReservationController : Controller
    {
        /// <summary>
        /// Display all reservations for a specific borrower
        /// GET: /Reservation/Index?borrowerId=STU001
        /// </summary>
        public IActionResult Index(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
            {
                // Show borrower selection if not provided
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                return View();
            }

            // Get reservations for this borrower
            try
            {
                var reservations = ReservationService.GetReservationsByBorrower(borrowerId);
                ViewBag.BorrowerId = borrowerId;
                ViewBag.BorrowerName = Borrower.AllBorrowers
                    .FirstOrDefault(b => b.BorrowerId == borrowerId)?.Name ?? borrowerId;
                return View(reservations);
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                return View(new List<Reservation>());
            }
        }

        /// <summary>
        /// Show form to create a new reservation
        /// GET: /Reservation/Create?borrowerId=STU001
        /// </summary>
        public IActionResult Create(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
            {
                // Show borrower selection form
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                return View();
            }

            // Check if borrower exists
            if (!Borrower.AllBorrowers.Any(b => b.BorrowerId == borrowerId))
            {
                ModelState.AddModelError("BorrowerId", "Selected borrower not found.");
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                return View();
            }

            // Get unavailable items for reservation
            var unavailableItems = Item.AllItems
                .Where(i => i.Status != "Available")
                .OrderBy(i => i.Name)
                .ToList();

            ViewBag.BorrowerId = borrowerId;
            ViewBag.BorrowerName = Borrower.AllBorrowers
                .FirstOrDefault(b => b.BorrowerId == borrowerId)?.Name ?? borrowerId;
            ViewBag.UnavailableItems = unavailableItems;

            return View();
        }

        /// <summary>
        /// Process reservation creation form submission
        /// POST: /Reservation/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string borrowerId, string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
            {
                ModelState.AddModelError("BorrowerId", "Please select or provide a borrower ID.");
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                return View();
            }

            if (string.IsNullOrWhiteSpace(itemLibraryCode))
            {
                ModelState.AddModelError("ItemLibraryCode", "Please select an item to reserve.");
                ViewBag.BorrowerId = borrowerId;
                ViewBag.BorrowerName = Borrower.AllBorrowers
                    .FirstOrDefault(b => b.BorrowerId == borrowerId)?.Name ?? borrowerId;
                ViewBag.UnavailableItems = Item.AllItems
                    .Where(i => i.Status != "Available")
                    .OrderBy(i => i.Name)
                    .ToList();
                return View();
            }

            try
            {
                var reservation = ReservationService.CreateReservation(borrowerId, itemLibraryCode);
                TempData["SuccessMessage"] = $"Reservation created successfully! ID: {reservation.ReservationId}";
                return RedirectToAction("Index", new { borrowerId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.BorrowerId = borrowerId;
                ViewBag.BorrowerName = Borrower.AllBorrowers
                    .FirstOrDefault(b => b.BorrowerId == borrowerId)?.Name ?? borrowerId;
                ViewBag.UnavailableItems = Item.AllItems
                    .Where(i => i.Status != "Available")
                    .OrderBy(i => i.Name)
                    .ToList();
                return View();
            }
        }

        /// <summary>
        /// Display details of a specific reservation
        /// GET: /Reservation/Details/RES001?borrowerId=STU001
        /// </summary>
        public IActionResult Details(string reservationId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(reservationId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                return BadRequest("ReservationId and BorrowerId are required.");
            }

            try
            {
                var reservation = ReservationService.GetReservationById(reservationId);
                if (reservation == null)
                {
                    return NotFound($"Reservation '{reservationId}' not found.");
                }

                // Authorization check: only the owner can view their reservation details
                if (reservation.BorrowerId != borrowerId)
                {
                    return Forbid();
                }

                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == reservation.ItemLibraryCode);
                var borrower = Borrower.AllBorrowers.FirstOrDefault(b => b.BorrowerId == borrowerId);

                ViewBag.Item = item;
                ViewBag.Borrower = borrower;
                ViewBag.CanCancel = reservation.Status == "Pending";

                return View(reservation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancel an existing reservation
        /// POST: /Reservation/Cancel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(string reservationId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(reservationId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                return BadRequest("ReservationId and BorrowerId are required.");
            }

            try
            {
                ReservationService.CancelReservation(reservationId, borrowerId);
                TempData["SuccessMessage"] = $"Reservation '{reservationId}' cancelled successfully.";
                return RedirectToAction("Index", new { borrowerId });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { reservationId, borrowerId });
            }
        }
    }
}
