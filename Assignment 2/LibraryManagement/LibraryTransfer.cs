using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Represents an inter-branch item transfer request.
    /// Enables coordination of item movement between physical library branches.
    /// </summary>
    public class LibraryTransfer
    {
        /// <summary>
        /// Unique transfer identifier (e.g., "TR001", "TR002").
        /// </summary>
        public string TransferId { get; set; }

        /// <summary>
        /// Unique identifier of the item being transferred.
        /// References Item.LibraryCode.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Branch ID where item currently is located.
        /// </summary>
        public string SourceBranchId { get; set; }

        /// <summary>
        /// Branch ID where item should be sent.
        /// </summary>
        public string DestinationBranchId { get; set; }

        /// <summary>
        /// Current status of the transfer in the workflow.
        /// </summary>
        public TransferStatus Status { get; set; }

        /// <summary>
        /// Timestamp when transfer was initiated.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Timestamp when transfer was completed/rejected (null if pending).
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Free-text notes about the transfer (e.g., rejection reason, special instructions).
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Static collection of all library transfers in memory.
        /// Persisted to transfers.json via DataService.
        /// </summary>
        public static List<LibraryTransfer> AllTransfers { get; } = new();

        /// <summary>
        /// Parameterless constructor for JSON deserialization.
        /// </summary>
        public LibraryTransfer()
        {
            CreatedDate = DateTime.Now;
            Notes = string.Empty;
        }

        /// <summary>
        /// Constructor with required fields.
        /// </summary>
        public LibraryTransfer(string transferId, string itemId, string sourceBranchId, string destinationBranchId, string notes = "")
        {
            if (string.IsNullOrWhiteSpace(transferId))
                throw new ArgumentException("TransferId cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(itemId))
                throw new ArgumentException("ItemId cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(sourceBranchId))
                throw new ArgumentException("SourceBranchId cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(destinationBranchId))
                throw new ArgumentException("DestinationBranchId cannot be null or empty.");
            if (sourceBranchId == destinationBranchId)
                throw new ArgumentException("Source and destination branches cannot be the same.");

            TransferId = transferId;
            ItemId = itemId;
            SourceBranchId = sourceBranchId;
            DestinationBranchId = destinationBranchId;
            Status = TransferStatus.Initiated;
            CreatedDate = DateTime.Now;
            CompletedDate = null;
            Notes = notes ?? string.Empty;
        }

        /// <summary>
        /// Generates human-readable summary of this transfer.
        /// </summary>
        public override string ToString()
        {
            return $"Transfer {TransferId}: Item {ItemId} from {SourceBranchId} to {DestinationBranchId} - Status: {Status}";
        }
    }

    /// <summary>
    /// Transfer workflow status.
    /// Initiated → Approved → In Transit → Completed
    /// Any status can transition to Rejected
    /// </summary>
    public enum TransferStatus
    {
        /// <summary>
        /// Transfer just created; awaiting approval at destination.
        /// </summary>
        Initiated = 0,

        /// <summary>
        /// Destination branch approved; item is being prepared for shipment.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Item is in transit between branches.
        /// </summary>
        InTransit = 2,

        /// <summary>
        /// Item arrived at destination; transfer complete. Item.BranchId updated.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Transfer rejected at any stage. Item remains at source branch.
        /// May include rejection notes in LibraryTransfer.Notes.
        /// </summary>
        Rejected = 4
    }
}
