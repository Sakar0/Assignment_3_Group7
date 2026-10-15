using Microsoft.AspNetCore.Mvc;
using Assignment2.LibraryManagement;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Controller for managing library branches
    /// Provides CRUD operations for branch data
    /// Accessible to staff for view, create, edit, and deactivate operations
    /// </summary>
    public class BranchController : Controller
    {
        private readonly BranchService _branchService;

        public BranchController()
        {
            _branchService = new BranchService();
        }

        /// <summary>
        /// Display list of all branches
        /// GET: /Branch/Index or /Branch
        /// </summary>
        public IActionResult Index()
        {
            var branches = _branchService.GetAllBranches();
            return View(branches);
        }

        /// <summary>
        /// Display create branch form
        /// GET: /Branch/Create
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handle branch creation form submission
        /// POST: /Branch/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BranchViewModel model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if BranchId already exists
            if (LibraryBranch.AllBranches.Any(b => b.BranchId == model.BranchId))
            {
                ModelState.AddModelError(nameof(model.BranchId), "A branch with this ID already exists.");
                return View(model);
            }

            try
            {
                bool success = _branchService.CreateBranch(model.BranchId, model.BranchName, model.Location);
                if (success)
                {
                    // Save changes to disk
                    DataService.SaveAllData();
                    TempData["SuccessMessage"] = $"Branch '{model.BranchName}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create branch. Please check your input.");
                    return View(model);
                }
            }
            catch (System.ArgumentException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// Display edit branch form
        /// GET: /Branch/Edit/{id}
        /// </summary>
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Branch ID is required.");
            }

            var branch = _branchService.FindBranchById(id);
            if (branch == null)
            {
                return NotFound($"Branch with ID '{id}' not found.");
            }

            var viewModel = new BranchViewModel
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                Location = branch.Location,
                IsActive = branch.IsActive
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handle branch edit form submission
        /// POST: /Branch/Edit/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, BranchViewModel model)
        {
            if (id != model.BranchId)
            {
                return BadRequest("Branch ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var branch = _branchService.FindBranchById(id);
            if (branch == null)
            {
                return NotFound($"Branch with ID '{id}' not found.");
            }

            try
            {
                // Update branch properties
                _branchService.UpdateBranch(id, "name", model.BranchName);
                _branchService.UpdateBranch(id, "location", model.Location);

                // Save changes to disk
                DataService.SaveAllData();
                TempData["SuccessMessage"] = $"Branch '{model.BranchName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.ArgumentException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// Handle branch deactivation
        /// POST: /Branch/Deactivate
        /// Only allows deactivating active branches
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deactivate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Branch ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var branch = _branchService.FindBranchById(id);
            if (branch == null)
            {
                TempData["ErrorMessage"] = $"Branch with ID '{id}' not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!branch.IsActive)
            {
                TempData["ErrorMessage"] = $"Branch '{branch.BranchName}' is already inactive.";
                return RedirectToAction(nameof(Index));
            }

            // Check if branch has active items
            int itemCount = _branchService.GetItemCountForBranch(id);
            if (itemCount > 0)
            {
                TempData["WarningMessage"] = $"Warning: Branch '{branch.BranchName}' has {itemCount} item(s). Items will remain associated with this branch.";
            }

            bool success = _branchService.DeactivateBranch(id);
            if (success)
            {
                DataService.SaveAllData();
                TempData["SuccessMessage"] = $"Branch '{branch.BranchName}' has been deactivated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to deactivate branch.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Handle branch reactivation
        /// POST: /Branch/Reactivate
        /// Only allows reactivating inactive branches
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reactivate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Branch ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var branch = _branchService.FindBranchById(id);
            if (branch == null)
            {
                TempData["ErrorMessage"] = $"Branch with ID '{id}' not found.";
                return RedirectToAction(nameof(Index));
            }

            if (branch.IsActive)
            {
                TempData["ErrorMessage"] = $"Branch '{branch.BranchName}' is already active.";
                return RedirectToAction(nameof(Index));
            }

            bool success = _branchService.ReactivateBranch(id);
            if (success)
            {
                DataService.SaveAllData();
                TempData["SuccessMessage"] = $"Branch '{branch.BranchName}' has been reactivated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reactivate branch.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// View model for branch data in forms
    /// </summary>
    public class BranchViewModel
    {
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
