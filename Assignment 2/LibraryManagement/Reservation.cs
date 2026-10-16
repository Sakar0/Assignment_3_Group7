using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Reservation class - represents a user's reservation for an unavailable item
    /// Allows borrowers to reserve items that are currently borrowed and be notified when available
    /// </summary>
    public class Reservation
    {
        public string ReservationId { get; set; }        // Unique identifier (e.g., "RES001")
        public string BorrowerId { get; set; }           // Who made the reservation
        public string ItemLibraryCode { get; set; }      // Which item is reserved
        public DateTime ReservationDate { get; set; }    // When the reservation was made
        public string Status { get; set; }               // Pending, Fulfilled, or Cancelled

        // Optional: when the reservation expires if not fulfilled (calculated on access)
        public DateTime? ExpiryDate { get; set; }

        // Shared list - stores ALL reservations in the system
        public static List<Reservation> AllReservations { get; } = new List<Reservation>();

        /// <summary>
        /// Create a new reservation
        /// </summary>
        public Reservation(string reservationId, string borrowerId, string itemLibraryCode, DateTime reservationDate, string status)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(reservationId))
                throw new ArgumentException("ReservationId cannot be empty.");
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            // Validate status
            ValidateStatus(status);

            ReservationId = reservationId;
            BorrowerId = borrowerId;
            ItemLibraryCode = itemLibraryCode;
            ReservationDate = reservationDate;
            Status = status;
            ExpiryDate = null; // Will be calculated as needed
        }

        /// <summary>
        /// Check if the status value is allowed
        /// Valid statuses: Pending, Fulfilled, Cancelled
        /// </summary>
        private void ValidateStatus(string status)
        {
            string[] allowedStatuses = { "Pending", "Fulfilled", "Cancelled" };
            if (!Array.Exists(allowedStatuses, element => element == status))
            {
                throw new ArgumentException($"Status must be one of: Pending, Fulfilled, Cancelled. Got: {status}");
            }
        }

        /// <summary>
        /// Is this reservation still active (not fulfilled or cancelled)?
        /// </summary>
        public bool IsActive()
        {
            return Status == "Pending";
        }

        /// <summary>
        /// Display reservation information
        /// </summary>
        public override string ToString()
        {
            return $"ReservationId: {ReservationId}, BorrowerId: {BorrowerId}, ItemCode: {ItemLibraryCode}, Status: {Status}, Date: {ReservationDate:yyyy-MM-dd}";
        }
    }
}
