using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Notification class - represents an in-application notification to a borrower
    /// Notifications are sent when reservation or waitlist status changes
    /// </summary>
    public class Notification
    {
        public string NotificationId { get; set; }       // Unique identifier (e.g., "NOTIF001")
        public string RecipientBorrowerId { get; set; }  // Who receives this notification
        public string Message { get; set; }              // The notification message
        public string Type { get; set; }                 // ReservationCreated, ReservationFulfilled, ReservationCancelled, OfferMade, OfferAccepted, OfferDeclined, OfferExpired, etc.
        public DateTime CreatedDate { get; set; }        // When the notification was created
        public bool IsRead { get; set; }                 // Has the user read this notification?
        public DateTime? ReadDate { get; set; }          // When the user marked it as read (if applicable)

        // Related item and reservation IDs (optional, for context)
        public string? ItemLibraryCode { get; set; }
        public string? ReservationId { get; set; }
        public string? WaitlistEntryId { get; set; }

        // Shared list - stores ALL notifications in the system
        public static List<Notification> AllNotifications { get; } = new List<Notification>();

        /// <summary>
        /// Create a new notification
        /// </summary>
        public Notification(string notificationId, string recipientBorrowerId, string message, string type, DateTime createdDate)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("NotificationId cannot be empty.");
            if (string.IsNullOrWhiteSpace(recipientBorrowerId))
                throw new ArgumentException("RecipientBorrowerId cannot be empty.");
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty.");
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type cannot be empty.");

            NotificationId = notificationId;
            RecipientBorrowerId = recipientBorrowerId;
            Message = message;
            Type = type;
            CreatedDate = createdDate;
            IsRead = false;
            ReadDate = null;
            ItemLibraryCode = null;
            ReservationId = null;
            WaitlistEntryId = null;
        }

        /// <summary>
        /// Mark this notification as read
        /// </summary>
        public void MarkAsRead()
        {
            IsRead = true;
            ReadDate = DateTime.Now;
        }

        /// <summary>
        /// Mark this notification as unread
        /// </summary>
        public void MarkAsUnread()
        {
            IsRead = false;
            ReadDate = null;
        }

        /// <summary>
        /// Display notification information
        /// </summary>
        public override string ToString()
        {
            string readStatus = IsRead ? "[READ]" : "[UNREAD]";
            return $"{readStatus} NotificationId: {NotificationId}, To: {RecipientBorrowerId}, Type: {Type}, Message: {Message}";
        }
    }
}
