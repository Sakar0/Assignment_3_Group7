using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service for managing library item transfers between branches.
    /// Handles CRUD operations and transfer workflow state transitions.
    /// </summary>
    public class TransferService
    {
        private static int _transferIdCounter = 1;

        /// <summary>
        /// Generates next transfer ID in sequence (TR001, TR002, etc.).
        /// </summary>
        private static string GenerateTransferId()
        {
            return $"TR{_transferIdCounter++:D3}";
        }

        /// <summary>
        /// Resets transfer ID counter (useful for testing or reinitialization).
        /// </summary>
        public static void ResetTransferIdCounter(int startValue = 1)
        {
            _transferIdCounter = startValue;
        }

        /// <summary>
        /// Initiates a new transfer request for an item.
        /// Item must exist, source and destination branches must be valid and different.
        /// Transfer starts in "Initiated" status, awaiting destination approval.
        /// </summary>
        public static LibraryTransfer InitiateTransfer(string itemId, string sourceBranchId, string destinationBranchId, string notes = "")
        {
            // Validate item exists
            var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == itemId);
            if (item == null)
                throw new InvalidOperationException($"Item {itemId} not found.");

            // Validate item is at source branch
            if (item.BranchId != sourceBranchId)
                throw new InvalidOperationException($"Item {itemId} is not at branch {sourceBranchId}. Currently at {item.BranchId}.");

            // Validate branches exist
            var sourceBranch = LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == sourceBranchId);
            var destBranch = LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == destinationBranchId);

            if (sourceBranch == null)
                throw new InvalidOperationException($"Source branch {sourceBranchId} not found.");
            if (destBranch == null)
                throw new InvalidOperationException($"Destination branch {destinationBranchId} not found.");

            // Validate branches are active
            if (!sourceBranch.IsActive)
                throw new InvalidOperationException($"Source branch {sourceBranchId} is not active.");
            if (!destBranch.IsActive)
                throw new InvalidOperationException($"Destination branch {destinationBranchId} is not active.");

            // Create transfer
            var transfer = new LibraryTransfer(
                GenerateTransferId(),
                itemId,
                sourceBranchId,
                destinationBranchId,
                notes
            );

            LibraryTransfer.AllTransfers.Add(transfer);
            Console.WriteLine($"[TransferService] Initiated: {transfer}");

            return transfer;
        }

        /// <summary>
        /// Approves a transfer at the destination branch.
        /// Transitions transfer from "Initiated" to "Approved".
        /// Only valid for transfers in "Initiated" status.
        /// </summary>
        public static LibraryTransfer ApproveTransfer(string transferId, string approvingBranchId, string notes = "")
        {
            var transfer = LibraryTransfer.AllTransfers.FirstOrDefault(t => t.TransferId == transferId);
            if (transfer == null)
                throw new InvalidOperationException($"Transfer {transferId} not found.");

            // Validate approving branch is the destination
            if (transfer.DestinationBranchId != approvingBranchId)
                throw new InvalidOperationException(
                    $"Only destination branch {transfer.DestinationBranchId} can approve. " +
                    $"Approving branch: {approvingBranchId}");

            if (transfer.Status != TransferStatus.Initiated)
                throw new InvalidOperationException(
                    $"Can only approve transfers in 'Initiated' status. Current status: {transfer.Status}");

            transfer.Status = TransferStatus.Approved;
            if (!string.IsNullOrEmpty(notes))
                transfer.Notes += $" [Approved: {notes}]";

            Console.WriteLine($"[TransferService] Approved: {transfer}");

            return transfer;
        }

        /// <summary>
        /// Rejects a transfer, keeping item at source branch.
        /// Transitions to "Rejected" status.
        /// Valid from any status except "Completed".
        /// </summary>
        public static LibraryTransfer RejectTransfer(string transferId, string rejectingBranchId, string rejectionReason = "")
        {
            var transfer = LibraryTransfer.AllTransfers.FirstOrDefault(t => t.TransferId == transferId);
            if (transfer == null)
                throw new InvalidOperationException($"Transfer {transferId} not found.");

            // Only source or destination can reject
            if (transfer.SourceBranchId != rejectingBranchId && transfer.DestinationBranchId != rejectingBranchId)
                throw new InvalidOperationException(
                    $"Only source or destination branch can reject. Rejecting branch: {rejectingBranchId}");

            if (transfer.Status == TransferStatus.Completed)
                throw new InvalidOperationException("Cannot reject a completed transfer.");

            transfer.Status = TransferStatus.Rejected;
            transfer.CompletedDate = DateTime.Now;
            transfer.Notes = $"Rejected: {rejectionReason}".TrimEnd(':');

            Console.WriteLine($"[TransferService] Rejected: {transfer}");

            return transfer;
        }

        /// <summary>
        /// Completes a transfer, updating item's BranchId to destination.
        /// Transitions transfer from "Approved" or "InTransit" to "Completed".
        /// Updates Item.BranchId atomically with transfer status.
        /// </summary>
        public static LibraryTransfer CompleteTransfer(string transferId, string completingBranchId)
        {
            var transfer = LibraryTransfer.AllTransfers.FirstOrDefault(t => t.TransferId == transferId);
            if (transfer == null)
                throw new InvalidOperationException($"Transfer {transferId} not found.");

            // Destination branch completes the transfer
            if (transfer.DestinationBranchId != completingBranchId)
                throw new InvalidOperationException(
                    $"Only destination branch {transfer.DestinationBranchId} can complete. " +
                    $"Completing branch: {completingBranchId}");

            if (transfer.Status != TransferStatus.Approved && transfer.Status != TransferStatus.InTransit)
                throw new InvalidOperationException(
                    $"Can only complete transfers in 'Approved' or 'InTransit' status. Current: {transfer.Status}");

            // Update item's branch location
            var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == transfer.ItemId);
            if (item == null)
                throw new InvalidOperationException($"Item {transfer.ItemId} not found.");

            item.BranchId = transfer.DestinationBranchId;

            // Mark transfer complete
            transfer.Status = TransferStatus.Completed;
            transfer.CompletedDate = DateTime.Now;

            Console.WriteLine($"[TransferService] Completed: {transfer}");
            Console.WriteLine($"[TransferService] Item {item.LibraryCode} now at branch {item.BranchId}");

            return transfer;
        }

        /// <summary>
        /// Retrieves all transfers for a specific branch (as source or destination).
        /// Includes transfers in all statuses.
        /// </summary>
        public static List<LibraryTransfer> GetTransfersForBranch(string branchId)
        {
            return LibraryTransfer.AllTransfers
                .Where(t => t.SourceBranchId == branchId || t.DestinationBranchId == branchId)
                .ToList();
        }

        /// <summary>
        /// Retrieves all outgoing transfers from a source branch.
        /// </summary>
        public static List<LibraryTransfer> GetOutgoingTransfers(string sourceBranchId)
        {
            return LibraryTransfer.AllTransfers
                .Where(t => t.SourceBranchId == sourceBranchId)
                .ToList();
        }

        /// <summary>
        /// Retrieves all incoming transfers to a destination branch.
        /// </summary>
        public static List<LibraryTransfer> GetIncomingTransfers(string destinationBranchId)
        {
            return LibraryTransfer.AllTransfers
                .Where(t => t.DestinationBranchId == destinationBranchId)
                .ToList();
        }

        /// <summary>
        /// Retrieves pending approval transfers (Initiated status) for a destination branch.
        /// Staff at destination use this to review and approve incoming transfer requests.
        /// </summary>
        public static List<LibraryTransfer> GetPendingApprovalsForBranch(string destinationBranchId)
        {
            return LibraryTransfer.AllTransfers
                .Where(t => t.DestinationBranchId == destinationBranchId && t.Status == TransferStatus.Initiated)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Retrieves complete transfer history with optional filtering.
        /// </summary>
        public static List<LibraryTransfer> GetTransferHistory(string filterBranchId = null, TransferStatus? filterStatus = null)
        {
            var query = LibraryTransfer.AllTransfers.AsEnumerable();

            if (!string.IsNullOrEmpty(filterBranchId))
                query = query.Where(t => t.SourceBranchId == filterBranchId || t.DestinationBranchId == filterBranchId);

            if (filterStatus.HasValue)
                query = query.Where(t => t.Status == filterStatus.Value);

            return query.OrderByDescending(t => t.CreatedDate).ToList();
        }

        /// <summary>
        /// Retrieves a single transfer by ID.
        /// </summary>
        public static LibraryTransfer GetTransferById(string transferId)
        {
            return LibraryTransfer.AllTransfers.FirstOrDefault(t => t.TransferId == transferId);
        }

        /// <summary>
        /// Retrieves all transfers for a specific item across all branches.
        /// Useful for tracking item movement history.
        /// </summary>
        public static List<LibraryTransfer> GetTransferHistoryForItem(string itemId)
        {
            return LibraryTransfer.AllTransfers
                .Where(t => t.ItemId == itemId)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();
        }

        /// <summary>
        /// Counts pending transfers awaiting approval at a destination branch.
        /// </summary>
        public static int CountPendingApprovalsForBranch(string destinationBranchId)
        {
            return LibraryTransfer.AllTransfers
                .Count(t => t.DestinationBranchId == destinationBranchId && t.Status == TransferStatus.Initiated);
        }

        /// <summary>
        /// Counts active transfers (Initiated, Approved, InTransit) for a branch.
        /// </summary>
        public static int CountActiveTransfersForBranch(string branchId)
        {
            return LibraryTransfer.AllTransfers
                .Count(t => (t.SourceBranchId == branchId || t.DestinationBranchId == branchId) &&
                           (t.Status == TransferStatus.Initiated || 
                            t.Status == TransferStatus.Approved || 
                            t.Status == TransferStatus.InTransit));
        }
    }
}
