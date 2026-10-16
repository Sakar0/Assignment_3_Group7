using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// WaitlistEntry class - represents a user waiting for an unavailable item to become available
    /// Maintains a queue position and tracks status (waiting, accepted offer, declined, expired)
    /// </summary>
    public class WaitlistEntry
    {
        public string WaitlistEntryId { get; set; }      // Unique identifier (e.g., "WAIT001")
        public string BorrowerId { get; set; }           // Who is waiting
        public string ItemLibraryCode { get; set; }      // Which item they're waiting for
        public int QueuePosition { get; set; }           // Position in the queue (1, 2, 3, ...)
        public DateTime CreatedDate { get; set; }        // When they joined the waitlist
        public string Status { get; set; }               // Waiting, OfferMade, Accepted, Declined, Expired, Cancelled
        public DateTime? OfferExpiryDate { get; set; }   // When the offer expires (if OfferMade)

        // Shared list - stores ALL waitlist entries in the system
        public static List<WaitlistEntry> AllWaitlistEntries { get; } = new List<WaitlistEntry>();

        /// <summary>
        /// Create a new waitlist entry
        /// </summary>
        public WaitlistEntry(string waitlistEntryId, string borrowerId, string itemLibraryCode, int queuePosition, DateTime createdDate, string status)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(waitlistEntryId))
                throw new ArgumentException("WaitlistEntryId cannot be empty.");
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");
            if (queuePosition < 1)
                throw new ArgumentException("QueuePosition must be at least 1.");

            // Validate status
            ValidateStatus(status);

            WaitlistEntryId = waitlistEntryId;
            BorrowerId = borrowerId;
            ItemLibraryCode = itemLibraryCode;
            QueuePosition = queuePosition;
            CreatedDate = createdDate;
            Status = status;
            OfferExpiryDate = null;
        }

        /// <summary>
        /// Check if the status value is allowed
        /// Valid statuses: Waiting, OfferMade, Accepted, Declined, Expired, Cancelled
        /// </summary>
        private void ValidateStatus(string status)
        {
            string[] allowedStatuses = { "Waiting", "OfferMade", "Accepted", "Declined", "Expired", "Cancelled" };
            if (!Array.Exists(allowedStatuses, element => element == status))
            {
                throw new ArgumentException($"Status must be one of: Waiting, OfferMade, Accepted, Declined, Expired, Cancelled. Got: {status}");
            }
        }

        /// <summary>
        /// Is this entry still actively waiting (not accepted, declined, expired, or cancelled)?
        /// </summary>
        public bool IsActive()
        {
            return Status == "Waiting" || Status == "OfferMade";
        }

        /// <summary>
        /// Has the offer (if made) expired?
        /// </summary>
        public bool HasOfferExpired()
        {
            if (Status != "OfferMade" || OfferExpiryDate == null)
                return false;

            return DateTime.Now > OfferExpiryDate;
        }

        /// <summary>
        /// Display waitlist entry information
        /// </summary>
        public override string ToString()
        {
            return $"WaitlistEntryId: {WaitlistEntryId}, BorrowerId: {BorrowerId}, ItemCode: {ItemLibraryCode}, Position: {QueuePosition}, Status: {Status}";
        }
    }
}
