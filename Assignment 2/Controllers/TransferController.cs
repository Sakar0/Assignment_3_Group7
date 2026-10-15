using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Assignment2.LibraryManagement;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Staff controller for managing inter-branch item transfers.
    /// Handles transfer initiation, approval, rejection, and completion workflows.
    /// </summary>
    public class TransferController : Controller
    {
        /// <summary>
        /// Display form to initiate a new transfer.
        /// Get available branches and items for selection.
        /// </summary>
        public IActionResult InitiateTransferForm()
        {
            var model = new InitiateTransferViewModel
            {
                Branches = LibraryBranch.AllBranches.Where(b => b.IsActive).OrderBy(b => b.BranchName).ToList(),
                AvailableItems = Item.AllItems.ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Handle transfer initiation form submission.
        /// Validates input and creates new transfer in "Initiated" status.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InitiateTransferForm(InitiateTransferViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedItemId) ||
                string.IsNullOrEmpty(model.SourceBranchId) ||
                string.IsNullOrEmpty(model.DestinationBranchId))
            {
                ModelState.AddModelError(string.Empty, "Please select item and branches.");
                model.Branches = LibraryBranch.AllBranches.Where(b => b.IsActive).OrderBy(b => b.BranchName).ToList();
                model.AvailableItems = Item.AllItems.ToList();
                return View(model);
            }

            try
            {
                var transfer = TransferService.InitiateTransfer(
                    model.SelectedItemId,
                    model.SourceBranchId,
                    model.DestinationBranchId,
                    model.Notes ?? ""
                );

                DataService.SaveAllData();

                TempData["SuccessMessage"] = $"Transfer {transfer.TransferId} created successfully. Awaiting approval at {LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == model.DestinationBranchId)?.BranchName}.";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error initiating transfer: {ex.Message}");
                model.Branches = LibraryBranch.AllBranches.Where(b => b.IsActive).OrderBy(b => b.BranchName).ToList();
                model.AvailableItems = Item.AllItems.ToList();
                return View(model);
            }
        }

        /// <summary>
        /// Display pending transfer approvals for the specified destination branch.
        /// Staff can approve or reject from this view.
        /// </summary>
        public IActionResult PendingApprovals(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
            {
                // If no branchId provided, redirect to first active branch's pending approvals
                var firstActiveBranch = LibraryBranch.AllBranches.FirstOrDefault(b => b.IsActive);
                if (firstActiveBranch != null)
                {
                    return RedirectToAction("PendingApprovals", new { branchId = firstActiveBranch.BranchId });
                }

                TempData["ErrorMessage"] = "No active branches available.";
                return RedirectToAction("History");
            }

            var branch = LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == branchId);
            if (branch == null)
            {
                TempData["ErrorMessage"] = "Branch not found.";
                return RedirectToAction("History");
            }

            var pendingTransfers = TransferService.GetPendingApprovalsForBranch(branchId);
            var model = new PendingApprovalsViewModel
            {
                DestinationBranch = branch,
                AllActiveBranches = LibraryBranch.AllBranches.Where(b => b.IsActive).OrderBy(b => b.BranchName).ToList(),
                PendingTransfers = pendingTransfers.Select(t => new PendingTransferDetailViewModel
                {
                    Transfer = t,
                    Item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == t.ItemId),
                    SourceBranch = LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == t.SourceBranchId)
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Handle approval of a transfer.
        /// Transitions transfer from "Initiated" to "Approved".
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveTransfer(string transferId, string approvingBranchId, string approvalNote = "")
        {
            if (string.IsNullOrEmpty(transferId) || string.IsNullOrEmpty(approvingBranchId))
            {
                TempData["ErrorMessage"] = "Invalid transfer or branch.";
                return RedirectToAction("PendingApprovals", new { branchId = approvingBranchId });
            }

            try
            {
                TransferService.ApproveTransfer(transferId, approvingBranchId, approvalNote);
                DataService.SaveAllData();

                TempData["SuccessMessage"] = $"Transfer {transferId} approved successfully.";
                return RedirectToAction("PendingApprovals", new { branchId = approvingBranchId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving transfer: {ex.Message}";
                return RedirectToAction("PendingApprovals", new { branchId = approvingBranchId });
            }
        }

        /// <summary>
        /// Handle rejection of a transfer.
        /// Can reject from any state. Item remains at source.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectTransfer(string transferId, string rejectingBranchId, string rejectionReason = "")
        {
            if (string.IsNullOrEmpty(transferId) || string.IsNullOrEmpty(rejectingBranchId))
            {
                TempData["ErrorMessage"] = "Invalid transfer or branch.";
                return RedirectToAction("History");
            }

            try
            {
                TransferService.RejectTransfer(transferId, rejectingBranchId, rejectionReason);
                DataService.SaveAllData();

                TempData["SuccessMessage"] = $"Transfer {transferId} rejected.";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting transfer: {ex.Message}";
                return RedirectToAction("History");
            }
        }

        /// <summary>
        /// Handle completion of a transfer.
        /// Destination branch marks transfer complete. Item.BranchId updated.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteTransfer(string transferId, string completingBranchId)
        {
            if (string.IsNullOrEmpty(transferId) || string.IsNullOrEmpty(completingBranchId))
            {
                TempData["ErrorMessage"] = "Invalid transfer or branch.";
                return RedirectToAction("History");
            }

            try
            {
                TransferService.CompleteTransfer(transferId, completingBranchId);
                DataService.SaveAllData();

                TempData["SuccessMessage"] = $"Transfer {transferId} completed. Item has been relocated.";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error completing transfer: {ex.Message}";
                return RedirectToAction("History");
            }
        }

        /// <summary>
        /// Display complete transfer history across all branches.
        /// Optionally filter by branch or status.
        /// </summary>
        public IActionResult History(string filterBranchId = "", string filterStatus = "")
        {
            var transfers = TransferService.GetTransferHistory();

            // Apply branch filter if specified
            if (!string.IsNullOrEmpty(filterBranchId))
            {
                transfers = transfers
                    .Where(t => t.SourceBranchId == filterBranchId || t.DestinationBranchId == filterBranchId)
                    .ToList();
            }

            // Apply status filter if specified
            if (!string.IsNullOrEmpty(filterStatus) && Enum.TryParse<TransferStatus>(filterStatus, out var status))
            {
                transfers = transfers.Where(t => t.Status == status).ToList();
            }

            var model = new TransferHistoryViewModel
            {
                Transfers = transfers.Select(t => new TransferDetailViewModel
                {
                    Transfer = t,
                    Item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == t.ItemId),
                    SourceBranch = LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == t.SourceBranchId),
                    DestinationBranch = LibraryBranch.AllBranches.FirstOrDefault(b => b.BranchId == t.DestinationBranchId)
                }).ToList(),
                AllBranches = LibraryBranch.AllBranches.Where(b => b.IsActive).OrderBy(b => b.BranchName).ToList(),
                SelectedBranchFilter = filterBranchId,
                SelectedStatusFilter = filterStatus
            };

            return View(model);
        }

        /// <summary>
        /// Get items currently at a specific branch (for dropdown population).
        /// Used by InitiateTransferForm to show only items at selected source.
        /// </summary>
        [HttpGet]
        public JsonResult GetItemsAtBranch(string branchId)
        {
            var items = Item.AllItems
                .Where(i => i.BranchId == branchId)
                .Select(i => new { id = i.LibraryCode, name = $"{i.Name} ({i.LibraryCode})" })
                .ToList();

            return Json(items);
        }
    }

    // ===== View Models =====

    /// <summary>
    /// Model for transfer initiation form.
    /// </summary>
    public class InitiateTransferViewModel
    {
        public string SelectedItemId { get; set; }
        public string SourceBranchId { get; set; }
        public string DestinationBranchId { get; set; }
        public string Notes { get; set; }
        public List<LibraryBranch> Branches { get; set; } = new();
        public List<Item> AvailableItems { get; set; } = new();
    }

    /// <summary>
    /// Model for pending approvals list.
    /// </summary>
    public class PendingApprovalsViewModel
    {
        public LibraryBranch DestinationBranch { get; set; }
        public List<PendingTransferDetailViewModel> PendingTransfers { get; set; } = new();
        public List<LibraryBranch> AllActiveBranches { get; set; } = new();
    }

    /// <summary>
    /// Model for single pending transfer detail.
    /// </summary>
    public class PendingTransferDetailViewModel
    {
        public LibraryTransfer Transfer { get; set; }
        public Item Item { get; set; }
        public LibraryBranch SourceBranch { get; set; }
    }

    /// <summary>
    /// Model for transfer history list.
    /// </summary>
    public class TransferHistoryViewModel
    {
        public List<TransferDetailViewModel> Transfers { get; set; } = new();
        public List<LibraryBranch> AllBranches { get; set; } = new();
        public string SelectedBranchFilter { get; set; }
        public string SelectedStatusFilter { get; set; }
    }

    /// <summary>
    /// Model for single transfer detail in history.
    /// </summary>
    public class TransferDetailViewModel
    {
        public LibraryTransfer Transfer { get; set; }
        public Item Item { get; set; }
        public LibraryBranch SourceBranch { get; set; }
        public LibraryBranch DestinationBranch { get; set; }
    }
}
