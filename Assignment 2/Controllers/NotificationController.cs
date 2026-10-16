using Assignment2.LibraryManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Controller for managing user notifications
    /// Handles viewing, marking as read/unread, and deleting notifications
    /// </summary>
    public class NotificationController : Controller
    {
        /// <summary>
        /// Display all notifications for a specific borrower
        /// GET: /Notification/Index?borrowerId=STU001
        /// </summary>
        public IActionResult Index(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
            {
                // Show borrower selection if not provided
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                return View(new List<Notification>());
            }

            // Validate borrower exists
            if (!Borrower.AllBorrowers.Any(b => b.BorrowerId == borrowerId))
            {
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                ViewBag.ErrorMessage = $"Borrower '{borrowerId}' not found.";
                return View(new List<Notification>());
            }

            // Get notifications for this borrower
            try
            {
                var notifications = NotificationService.GetNotificationsByBorrower(borrowerId);
                ViewBag.BorrowerId = borrowerId;
                ViewBag.BorrowerName = Borrower.AllBorrowers
                    .FirstOrDefault(b => b.BorrowerId == borrowerId)?.Name ?? borrowerId;
                ViewBag.UnreadCount = NotificationService.GetUnreadCountByBorrower(borrowerId);
                return View(notifications);
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.Borrowers = Borrower.AllBorrowers.OrderBy(b => b.Name).ToList();
                return View(new List<Notification>());
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// POST: /Notification/MarkAsRead
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsRead(string notificationId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(notificationId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                return BadRequest("NotificationId and BorrowerId are required.");
            }

            try
            {
                // Verify the notification belongs to the borrower
                var notification = NotificationService.GetNotificationById(notificationId);
                if (notification == null)
                {
                    return NotFound($"Notification '{notificationId}' not found.");
                }

                // Authorization check: only the recipient can mark it as read
                if (notification.RecipientBorrowerId != borrowerId)
                {
                    return Forbid();
                }

                NotificationService.MarkAsRead(notificationId);
                TempData["SuccessMessage"] = "Notification marked as read.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("Index", new { borrowerId });
        }

        /// <summary>
        /// Mark a notification as unread
        /// POST: /Notification/MarkAsUnread
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsUnread(string notificationId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(notificationId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                return BadRequest("NotificationId and BorrowerId are required.");
            }

            try
            {
                // Verify the notification belongs to the borrower
                var notification = NotificationService.GetNotificationById(notificationId);
                if (notification == null)
                {
                    return NotFound($"Notification '{notificationId}' not found.");
                }

                // Authorization check: only the recipient can mark it as unread
                if (notification.RecipientBorrowerId != borrowerId)
                {
                    return Forbid();
                }

                NotificationService.MarkAsUnread(notificationId);
                TempData["SuccessMessage"] = "Notification marked as unread.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("Index", new { borrowerId });
        }

        /// <summary>
        /// Delete a notification
        /// POST: /Notification/Delete
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string notificationId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(notificationId) || string.IsNullOrWhiteSpace(borrowerId))
            {
                return BadRequest("NotificationId and BorrowerId are required.");
            }

            try
            {
                // Verify the notification belongs to the borrower
                var notification = NotificationService.GetNotificationById(notificationId);
                if (notification == null)
                {
                    return NotFound($"Notification '{notificationId}' not found.");
                }

                // Authorization check: only the recipient can delete it
                if (notification.RecipientBorrowerId != borrowerId)
                {
                    return Forbid();
                }

                NotificationService.DeleteNotification(notificationId);
                TempData["SuccessMessage"] = "Notification deleted.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("Index", new { borrowerId });
        }
    }
}
