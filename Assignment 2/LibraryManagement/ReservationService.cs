using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service for managing reservation operations
    /// Handles creation, retrieval, cancellation, and validation of reservations
    /// </summary>
    public static class ReservationService
    {
        /// <summary>
        /// Generate a unique reservation ID
        /// Format: RES + sequential number (RES001, RES002, etc.)
        /// </summary>
        private static string GenerateReservationId()
        {
            int nextNumber = Reservation.AllReservations.Count + 1;
            return $"RES{nextNumber:D3}";
        }

        /// <summary>
        /// Check if an item exists in the library
        /// </summary>
        private static bool IsItemValid(string itemLibraryCode)
        {
            return Item.AllItems.Any(i => i.LibraryCode == itemLibraryCode);
        }

        /// <summary>
        /// Check if a borrower exists in the system
        /// </summary>
        private static bool IsBorrowerValid(string borrowerId)
        {
            return Borrower.AllBorrowers.Any(b => b.BorrowerId == borrowerId);
        }

        /// <summary>
        /// Check if an item is currently available
        /// An item is available if its status is "Available"
        /// </summary>
        private static bool IsItemAvailable(string itemLibraryCode)
        {
            var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == itemLibraryCode);
            return item != null && item.Status == "Available";
        }

        /// <summary>
        /// Get an item by its library code
        /// </summary>
        private static Item? GetItemByCode(string itemLibraryCode)
        {
            return Item.AllItems.FirstOrDefault(i => i.LibraryCode == itemLibraryCode);
        }

        /// <summary>
        /// Get a borrower by their ID
        /// </summary>
        private static Borrower? GetBorrowerById(string borrowerId)
        {
            return Borrower.AllBorrowers.FirstOrDefault(b => b.BorrowerId == borrowerId);
        }

        /// <summary>
        /// Check if a borrower already has an active reservation for an item
        /// Returns the existing reservation if found, null otherwise
        /// </summary>
        public static Reservation? FindActiveReservationForItem(string borrowerId, string itemLibraryCode)
        {
            return Reservation.AllReservations.FirstOrDefault(r =>
                r.BorrowerId == borrowerId &&
                r.ItemLibraryCode == itemLibraryCode &&
                r.Status == "Pending");
        }

        /// <summary>
        /// Check if a borrower has any active reservations for an item
        /// (regardless of whether there are pending offers in the waitlist)
        /// </summary>
        public static bool HasActiveReservation(string borrowerId, string itemLibraryCode)
        {
            return FindActiveReservationForItem(borrowerId, itemLibraryCode) != null;
        }

        /// <summary>
        /// Get all reservations for a specific borrower
        /// Includes pending, fulfilled, and cancelled reservations
        /// </summary>
        public static List<Reservation> GetReservationsByBorrower(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            return Reservation.AllReservations
                .Where(r => r.BorrowerId == borrowerId)
                .OrderByDescending(r => r.ReservationDate)
                .ToList();
        }

        /// <summary>
        /// Get all active (pending) reservations for a specific borrower
        /// </summary>
        public static List<Reservation> GetActiveReservationsByBorrower(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            return Reservation.AllReservations
                .Where(r => r.BorrowerId == borrowerId && r.Status == "Pending")
                .OrderByDescending(r => r.ReservationDate)
                .ToList();
        }

        /// <summary>
        /// Get a specific reservation by its ID
        /// </summary>
        public static Reservation? GetReservationById(string reservationId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
                throw new ArgumentException("ReservationId cannot be empty.");

            return Reservation.AllReservations.FirstOrDefault(r => r.ReservationId == reservationId);
        }

        /// <summary>
        /// Get all reservations for a specific item
        /// </summary>
        public static List<Reservation> GetReservationsByItem(string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            return Reservation.AllReservations
                .Where(r => r.ItemLibraryCode == itemLibraryCode)
                .OrderBy(r => r.ReservationDate)
                .ToList();
        }

        /// <summary>
        /// Get all active reservations for a specific item
        /// </summary>
        public static List<Reservation> GetActiveReservationsByItem(string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            return Reservation.AllReservations
                .Where(r => r.ItemLibraryCode == itemLibraryCode && r.Status == "Pending")
                .OrderBy(r => r.ReservationDate)
                .ToList();
        }

        /// <summary>
        /// Create a new reservation for a borrower
        /// Validates that:
        /// - Both borrower and item exist
        /// - Item is currently unavailable (not Available status)
        /// - Borrower doesn't already have an active reservation for this item
        /// Returns the created reservation on success
        /// Throws ArgumentException on validation failure
        /// </summary>
        public static Reservation CreateReservation(string borrowerId, string itemLibraryCode)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            // Check if borrower exists
            if (!IsBorrowerValid(borrowerId))
                throw new ArgumentException($"Borrower '{borrowerId}' not found in the system.");

            // Check if item exists
            if (!IsItemValid(itemLibraryCode))
                throw new ArgumentException($"Item '{itemLibraryCode}' not found in the library.");

            // Check if item is available (cannot reserve available items)
            if (IsItemAvailable(itemLibraryCode))
                throw new ArgumentException($"Item '{itemLibraryCode}' is currently available and does not need to be reserved.");

            // Check for duplicate active reservation
            if (HasActiveReservation(borrowerId, itemLibraryCode))
                throw new ArgumentException($"Borrower '{borrowerId}' already has an active reservation for item '{itemLibraryCode}'.");

            // Create the reservation
            string reservationId = GenerateReservationId();
            var reservation = new Reservation(
                reservationId,
                borrowerId,
                itemLibraryCode,
                DateTime.Now,
                "Pending"
            );

            Reservation.AllReservations.Add(reservation);

            // Create notification for the borrower
            try
            {
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == itemLibraryCode);
                string itemName = item?.Name ?? itemLibraryCode;
                string message = $"Your reservation for '{itemName}' has been created and is pending. You will be notified when it's ready for pickup.";

                NotificationService.CreateNotification(
                    borrowerId,
                    message,
                    "ReservationCreated",
                    itemLibraryCode,
                    reservationId
                );
            }
            catch (Exception ex)
            {
                // Log but don't fail the reservation if notification creation fails
                Console.WriteLine($"Warning: Failed to create reservation notification: {ex.Message}");
            }

            return reservation;
        }

        /// <summary>
        /// Cancel an existing reservation
        /// Only the borrower who made the reservation can cancel it
        /// Can only cancel pending reservations
        /// Throws ArgumentException if:
        /// - Reservation not found
        /// - User is not authorized to cancel (not the owner)
        /// - Reservation is not in Pending status
        /// </summary>
        public static void CancelReservation(string reservationId, string requestingBorrowerId)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(reservationId))
                throw new ArgumentException("ReservationId cannot be empty.");
            if (string.IsNullOrWhiteSpace(requestingBorrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            // Find the reservation
            var reservation = GetReservationById(reservationId);
            if (reservation == null)
                throw new ArgumentException($"Reservation '{reservationId}' not found.");

            // Authorization check: only the owner can cancel their own reservation
            if (reservation.BorrowerId != requestingBorrowerId)
                throw new ArgumentException($"You are not authorized to cancel this reservation. Only the reservation owner can cancel.");

            // Status check: can only cancel pending reservations
            if (reservation.Status != "Pending")
                throw new ArgumentException($"Cannot cancel reservation with status '{reservation.Status}'. Only pending reservations can be cancelled.");

            // Update status
            reservation.Status = "Cancelled";

            // Create notification for cancellation
            try
            {
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == reservation.ItemLibraryCode);
                string itemName = item?.Name ?? reservation.ItemLibraryCode;
                string message = $"Your reservation for '{itemName}' has been cancelled.";

                NotificationService.CreateNotification(
                    requestingBorrowerId,
                    message,
                    "ReservationCancelled",
                    reservation.ItemLibraryCode,
                    reservationId,
                    null
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create cancellation notification: {ex.Message}");
                // Do not throw - cancellation succeeded even if notification fails
            }
        }

        /// <summary>
        /// Fulfill a reservation (called when the item becomes available)
        /// This marks the reservation as Fulfilled and may trigger waitlist processing
        /// </summary>
        public static void FulfillReservation(string reservationId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
                throw new ArgumentException("ReservationId cannot be empty.");

            var reservation = GetReservationById(reservationId);
            if (reservation == null)
                throw new ArgumentException($"Reservation '{reservationId}' not found.");

            if (reservation.Status != "Pending")
                throw new ArgumentException($"Cannot fulfill reservation with status '{reservation.Status}'.");

            reservation.Status = "Fulfilled";

            // Create notification for the borrower
            try
            {
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == reservation.ItemLibraryCode);
                string itemName = item?.Name ?? reservation.ItemLibraryCode;
                string message = $"Your reservation for '{itemName}' is now ready! Please visit the circulation desk within 7 days to pick it up.";

                NotificationService.CreateNotification(
                    reservation.BorrowerId,
                    message,
                    "ReservationFulfilled",
                    reservation.ItemLibraryCode,
                    reservationId
                );
            }
            catch (Exception ex)
            {
                // Log but don't fail the fulfillment if notification creation fails
                Console.WriteLine($"Warning: Failed to create fulfillment notification: {ex.Message}");
            }
        }
    }
}

