using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service for managing notifications
    /// Handles creation, retrieval, marking as read/unread, and deletion
    /// </summary>
    public static class NotificationService
    {
        /// <summary>
        /// Generate a unique notification ID
        /// Format: NOTIF + sequential number (NOTIF001, NOTIF002, etc.)
        /// </summary>
        private static string GenerateNotificationId()
        {
            int nextNumber = Notification.AllNotifications.Count + 1;
            return $"NOTIF{nextNumber:D3}";
        }

        /// <summary>
        /// Create and send a notification to a borrower
        /// </summary>
        public static Notification CreateNotification(
            string recipientBorrowerId,
            string message,
            string type,
            string? itemLibraryCode = null,
            string? reservationId = null,
            string? waitlistEntryId = null)
        {
            if (string.IsNullOrWhiteSpace(recipientBorrowerId))
                throw new ArgumentException("RecipientBorrowerId cannot be empty.");
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty.");
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type cannot be empty.");

            // Validate borrower exists
            if (!Borrower.AllBorrowers.Any(b => b.BorrowerId == recipientBorrowerId))
                throw new ArgumentException($"Borrower '{recipientBorrowerId}' not found.");

            string notificationId = GenerateNotificationId();
            var notification = new Notification(notificationId, recipientBorrowerId, message, type, DateTime.Now);

            // Set optional context fields
            if (!string.IsNullOrWhiteSpace(itemLibraryCode))
                notification.ItemLibraryCode = itemLibraryCode;

            if (!string.IsNullOrWhiteSpace(reservationId))
                notification.ReservationId = reservationId;

            if (!string.IsNullOrWhiteSpace(waitlistEntryId))
                notification.WaitlistEntryId = waitlistEntryId;

            Notification.AllNotifications.Add(notification);
            return notification;
        }

        /// <summary>
        /// Get a specific notification by ID
        /// </summary>
        public static Notification? GetNotificationById(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("NotificationId cannot be empty.");

            return Notification.AllNotifications.FirstOrDefault(n => n.NotificationId == notificationId);
        }

        /// <summary>
        /// Get all notifications for a specific borrower
        /// Sorted by CreatedDate (newest first)
        /// </summary>
        public static List<Notification> GetNotificationsByBorrower(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            return Notification.AllNotifications
                .Where(n => n.RecipientBorrowerId == borrowerId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Get unread notifications for a specific borrower
        /// </summary>
        public static List<Notification> GetUnreadNotificationsByBorrower(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            return Notification.AllNotifications
                .Where(n => n.RecipientBorrowerId == borrowerId && !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Get notifications of a specific type for a borrower
        /// </summary>
        public static List<Notification> GetNotificationsByBorrowerAndType(string borrowerId, string type)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type cannot be empty.");

            return Notification.AllNotifications
                .Where(n => n.RecipientBorrowerId == borrowerId && n.Type == type)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Get notifications related to a specific item
        /// </summary>
        public static List<Notification> GetNotificationsByItem(string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            return Notification.AllNotifications
                .Where(n => n.ItemLibraryCode == itemLibraryCode)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Get notifications related to a specific reservation
        /// </summary>
        public static List<Notification> GetNotificationsByReservation(string reservationId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
                throw new ArgumentException("ReservationId cannot be empty.");

            return Notification.AllNotifications
                .Where(n => n.ReservationId == reservationId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Get notifications related to a specific waitlist entry
        /// </summary>
        public static List<Notification> GetNotificationsByWaitlistEntry(string waitlistEntryId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId))
                throw new ArgumentException("WaitlistEntryId cannot be empty.");

            return Notification.AllNotifications
                .Where(n => n.WaitlistEntryId == waitlistEntryId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        public static void MarkAsRead(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("NotificationId cannot be empty.");

            var notification = GetNotificationById(notificationId);
            if (notification == null)
                throw new ArgumentException($"Notification '{notificationId}' not found.");

            notification.MarkAsRead();
        }

        /// <summary>
        /// Mark a notification as unread
        /// </summary>
        public static void MarkAsUnread(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("NotificationId cannot be empty.");

            var notification = GetNotificationById(notificationId);
            if (notification == null)
                throw new ArgumentException($"Notification '{notificationId}' not found.");

            notification.MarkAsUnread();
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        public static void DeleteNotification(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("NotificationId cannot be empty.");

            var notification = GetNotificationById(notificationId);
            if (notification == null)
                throw new ArgumentException($"Notification '{notificationId}' not found.");

            Notification.AllNotifications.Remove(notification);
        }

        /// <summary>
        /// Get count of unread notifications for a borrower
        /// </summary>
        public static int GetUnreadCountByBorrower(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            return Notification.AllNotifications
                .Count(n => n.RecipientBorrowerId == borrowerId && !n.IsRead);
        }
    }
}
