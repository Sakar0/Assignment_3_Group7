using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service for managing the waitlist queue
    /// Handles joining queue, processing offers, accepting/declining, and expiration
    /// </summary>
    public static class WaitlistService
    {
        // Default offer expiration time: 7 days
        private static readonly int OFFER_EXPIRATION_DAYS = 7;

        /// <summary>
        /// Generate a unique waitlist entry ID
        /// Format: WAIT + sequential number (WAIT001, WAIT002, etc.)
        /// </summary>
        private static string GenerateWaitlistEntryId()
        {
            int nextNumber = WaitlistEntry.AllWaitlistEntries.Count + 1;
            return $"WAIT{nextNumber:D3}";
        }

        /// <summary>
        /// Calculate the next queue position for an item
        /// Position is based on the highest existing position + 1
        /// </summary>
        private static int CalculateNextQueuePosition(string itemLibraryCode)
        {
            var existingEntries = WaitlistEntry.AllWaitlistEntries
                .Where(w => w.ItemLibraryCode == itemLibraryCode && w.IsActive())
                .ToList();

            if (existingEntries.Count == 0)
                return 1;

            return existingEntries.Max(w => w.QueuePosition) + 1;
        }

        /// <summary>
        /// Recalculate queue positions for all active entries of an item
        /// Called after removals or acceptances to maintain sequential positions
        /// </summary>
        private static void RecalculateQueuePositions(string itemLibraryCode)
        {
            var activeEntries = WaitlistEntry.AllWaitlistEntries
                .Where(w => w.ItemLibraryCode == itemLibraryCode && w.IsActive())
                .OrderBy(w => w.CreatedDate)
                .ToList();

            for (int i = 0; i < activeEntries.Count; i++)
            {
                activeEntries[i].QueuePosition = i + 1;
            }
        }

        /// <summary>
        /// Get all expired offers and optionally expire them
        /// </summary>
        private static List<WaitlistEntry> GetExpiredOffers()
        {
            return WaitlistEntry.AllWaitlistEntries
                .Where(w => w.Status == "OfferMade" && w.HasOfferExpired())
                .ToList();
        }

        /// <summary>
        /// Clean up expired offers by marking them as Expired
        /// Called periodically or before queue processing
        /// Creates OfferExpired notifications to notify borrowers
        /// </summary>
        public static void ExpireOldOffers()
        {
            var expiredOffers = GetExpiredOffers();
            foreach (var entry in expiredOffers)
            {
                entry.Status = "Expired";

                // Create notification for expiration (only once per entry)
                // Check if we already sent an expiration notification for this entry
                try
                {
                    var existingNotification = Notification.AllNotifications
                        .FirstOrDefault(n => n.WaitlistEntryId == entry.WaitlistEntryId &&
                                            n.Type == "OfferExpired" &&
                                            n.RecipientBorrowerId == entry.BorrowerId);

                    if (existingNotification == null)
                    {
                        var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == entry.ItemLibraryCode);
                        string itemName = item?.Name ?? entry.ItemLibraryCode;
                        string message = $"Your offer for '{itemName}' has expired as you did not accept it within 7 days. Please rejoin the waitlist if still interested.";

                        NotificationService.CreateNotification(
                            entry.BorrowerId,
                            message,
                            "OfferExpired",
                            entry.ItemLibraryCode,
                            null,
                            entry.WaitlistEntryId
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to create offer expiry notification for {entry.WaitlistEntryId}: {ex.Message}");
                    // Do not throw - expiration succeeded even if notification fails
                }
            }
        }

        /// <summary>
        /// Add a borrower to the waitlist for an item
        /// Validates borrower and item exist, and that item is currently unavailable
        /// </summary>
        public static WaitlistEntry AddToWaitlist(string borrowerId, string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            // Validate borrower exists
            if (!Borrower.AllBorrowers.Any(b => b.BorrowerId == borrowerId))
                throw new ArgumentException($"Borrower '{borrowerId}' not found.");

            // Validate item exists
            if (!Item.AllItems.Any(i => i.LibraryCode == itemLibraryCode))
                throw new ArgumentException($"Item '{itemLibraryCode}' not found.");

            // Check if borrower already has an active entry in queue for this item
            var existing = WaitlistEntry.AllWaitlistEntries.FirstOrDefault(w =>
                w.BorrowerId == borrowerId &&
                w.ItemLibraryCode == itemLibraryCode &&
                w.IsActive());

            if (existing != null)
                throw new ArgumentException($"Borrower '{borrowerId}' already has an active waitlist entry for item '{itemLibraryCode}'.");

            // Create new waitlist entry
            string entryId = GenerateWaitlistEntryId();
            int position = CalculateNextQueuePosition(itemLibraryCode);

            var entry = new WaitlistEntry(
                entryId,
                borrowerId,
                itemLibraryCode,
                position,
                DateTime.Now,
                "Waiting"
            );

            WaitlistEntry.AllWaitlistEntries.Add(entry);
            return entry;
        }

        /// <summary>
        /// Remove a borrower from the waitlist
        /// </summary>
        public static void RemoveFromWaitlist(string waitlistEntryId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId))
                throw new ArgumentException("WaitlistEntryId cannot be empty.");

            var entry = WaitlistEntry.AllWaitlistEntries.FirstOrDefault(w => w.WaitlistEntryId == waitlistEntryId);
            if (entry == null)
                throw new ArgumentException($"Waitlist entry '{waitlistEntryId}' not found.");

            string itemCode = entry.ItemLibraryCode;
            WaitlistEntry.AllWaitlistEntries.Remove(entry);

            // Recalculate positions for remaining entries
            RecalculateQueuePositions(itemCode);
        }

        /// <summary>
        /// Get the next person in queue for an item (first waiting entry)
        /// </summary>
        public static WaitlistEntry? GetNextInQueue(string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            return WaitlistEntry.AllWaitlistEntries
                .Where(w => w.ItemLibraryCode == itemLibraryCode && w.Status == "Waiting")
                .OrderBy(w => w.QueuePosition)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get all queued entries for an item (active only)
        /// </summary>
        public static List<WaitlistEntry> GetQueueForItem(string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            return WaitlistEntry.AllWaitlistEntries
                .Where(w => w.ItemLibraryCode == itemLibraryCode && w.IsActive())
                .OrderBy(w => w.QueuePosition)
                .ToList();
        }

        /// <summary>
        /// Get all waitlist entries for a specific borrower
        /// </summary>
        public static List<WaitlistEntry> GetWaitlistEntriesByBorrower(string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            return WaitlistEntry.AllWaitlistEntries
                .Where(w => w.BorrowerId == borrowerId)
                .OrderBy(w => w.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Get active waitlist entry by ID
        /// </summary>
        public static WaitlistEntry? GetWaitlistEntryById(string waitlistEntryId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId))
                throw new ArgumentException("WaitlistEntryId cannot be empty.");

            return WaitlistEntry.AllWaitlistEntries.FirstOrDefault(w => w.WaitlistEntryId == waitlistEntryId);
        }

        /// <summary>
        /// Process the waitlist queue when an item becomes available
        /// Makes an offer to the first person in queue (if any)
        /// Should be called when an item is returned via ReceptionService
        /// </summary>
        public static void ProcessQueue(string itemLibraryCode)
        {
            if (string.IsNullOrWhiteSpace(itemLibraryCode))
                throw new ArgumentException("ItemLibraryCode cannot be empty.");

            // Expire any old offers first
            ExpireOldOffers();

            // Get the next person in queue
            var nextEntry = GetNextInQueue(itemLibraryCode);
            if (nextEntry == null)
            {
                // No one waiting, nothing to do
                return;
            }

            // Make an offer to the next person
            nextEntry.Status = "OfferMade";
            nextEntry.OfferExpiryDate = DateTime.Now.AddDays(OFFER_EXPIRATION_DAYS);

            // Create notification for the borrower
            try
            {
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == itemLibraryCode);
                string itemName = item?.Name ?? itemLibraryCode;
                string message = $"An item you're waiting for is now available! '{itemName}' has been offered to you. You have 7 days to accept or decline this offer.";

                NotificationService.CreateNotification(
                    nextEntry.BorrowerId,
                    message,
                    "OfferMade",
                    itemLibraryCode,
                    null,
                    nextEntry.WaitlistEntryId
                );
            }
            catch (Exception ex)
            {
                // Log but don't fail the queue processing if notification creation fails
                Console.WriteLine($"Warning: Failed to create offer notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Accept an offer (borrower agrees to take the item)
        /// Marks the entry as Accepted
        /// </summary>
        public static void AcceptOffer(string waitlistEntryId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId))
                throw new ArgumentException("WaitlistEntryId cannot be empty.");
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            var entry = GetWaitlistEntryById(waitlistEntryId);
            if (entry == null)
                throw new ArgumentException($"Waitlist entry '{waitlistEntryId}' not found.");

            // Authorization check
            if (entry.BorrowerId != borrowerId)
                throw new ArgumentException("Only the offered user can accept this offer.");

            // Status check
            if (entry.Status != "OfferMade")
                throw new ArgumentException($"Cannot accept offer with status '{entry.Status}'. Offer must be in 'OfferMade' status.");

            // Check if offer has expired
            if (entry.HasOfferExpired())
            {
                entry.Status = "Expired";
                throw new ArgumentException("This offer has expired. Please contact the library for assistance.");
            }

            entry.Status = "Accepted";

            // Create notification for offer accepted
            try
            {
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == entry.ItemLibraryCode);
                string itemName = item?.Name ?? entry.ItemLibraryCode;
                string message = $"Your offer for '{itemName}' has been accepted. Please pick up the item at your earliest convenience.";

                NotificationService.CreateNotification(
                    borrowerId,
                    message,
                    "OfferAccepted",
                    entry.ItemLibraryCode,
                    null,
                    waitlistEntryId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create offer accepted notification: {ex.Message}");
                // Do not throw - acceptance succeeded even if notification fails
            }
        }

        /// <summary>
        /// Decline an offer (borrower does not want the item)
        /// Removes from queue and processes next person
        /// </summary>
        public static void DeclineOffer(string waitlistEntryId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId))
                throw new ArgumentException("WaitlistEntryId cannot be empty.");
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            var entry = GetWaitlistEntryById(waitlistEntryId);
            if (entry == null)
                throw new ArgumentException($"Waitlist entry '{waitlistEntryId}' not found.");

            // Authorization check
            if (entry.BorrowerId != borrowerId)
                throw new ArgumentException("Only the offered user can decline this offer.");

            // Status check
            if (entry.Status != "OfferMade")
                throw new ArgumentException($"Cannot decline offer with status '{entry.Status}'. Offer must be in 'OfferMade' status.");

            string itemCode = entry.ItemLibraryCode;

            // Remove from queue
            entry.Status = "Declined";
            WaitlistEntry.AllWaitlistEntries.Remove(entry);

            // Create notification for the declining borrower
            try
            {
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == itemCode);
                string itemName = item?.Name ?? itemCode;
                string message = $"You declined the offer for '{itemName}'. The item has been offered to the next person in the queue.";

                NotificationService.CreateNotification(
                    borrowerId,
                    message,
                    "OfferDeclined",
                    itemCode,
                    null,
                    waitlistEntryId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create decline notification: {ex.Message}");
            }

            // Recalculate positions
            RecalculateQueuePositions(itemCode);

            // Process next in queue
            ProcessQueue(itemCode);
        }

        /// <summary>
        /// Cancel a waitlist entry (borrower cancels their place in queue)
        /// Similar to decline but called on pending entries
        /// </summary>
        public static void CancelWaitlistEntry(string waitlistEntryId, string borrowerId)
        {
            if (string.IsNullOrWhiteSpace(waitlistEntryId))
                throw new ArgumentException("WaitlistEntryId cannot be empty.");
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            var entry = GetWaitlistEntryById(waitlistEntryId);
            if (entry == null)
                throw new ArgumentException($"Waitlist entry '{waitlistEntryId}' not found.");

            // Authorization check
            if (entry.BorrowerId != borrowerId)
                throw new ArgumentException("Only the waitlist entry owner can cancel it.");

            // Can only cancel waiting or offered entries
            if (entry.Status != "Waiting" && entry.Status != "OfferMade")
                throw new ArgumentException($"Cannot cancel entry with status '{entry.Status}'.");

            string itemCode = entry.ItemLibraryCode;
            var wasOffered = entry.Status == "OfferMade";

            // Remove from queue
            entry.Status = "Cancelled";
            WaitlistEntry.AllWaitlistEntries.Remove(entry);

            // Create notification for the cancelling borrower
            try
            {
                var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == itemCode);
                string itemName = item?.Name ?? itemCode;
                string message = $"You cancelled your place in the queue for '{itemName}'.";

                NotificationService.CreateNotification(
                    borrowerId,
                    message,
                    "WaitlistCancelled",
                    itemCode,
                    null,
                    waitlistEntryId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create cancellation notification: {ex.Message}");
            }

            // Recalculate positions
            RecalculateQueuePositions(itemCode);

            // If this user had been offered, process next
            if (wasOffered)
            {
                ProcessQueue(itemCode);
            }
        }
    }
}
