using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Assignment2.LibraryManagement;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Controller for catalogue import functionality
    /// Provides web interface for administrators to bulk import items from CSV files
    /// </summary>
    public class ImportController : Controller
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB max file size
        private const string AllowedMimeType = "text/csv";
        private const string AllowedMimeTypeAlternate = "application/csv";
        private readonly CatalogueImportService _importService = new CatalogueImportService();

        /// <summary>
        /// Displays the CSV import form
        /// GET: /Import/Index
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Handles CSV file upload and import
        /// POST: /Import/Upload
        /// </summary>
        [HttpPost]
        public IActionResult Upload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            // Validate file was provided
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file to upload.");
                return View("Index");
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError("file", $"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)} MB.");
                return View("Index");
            }

            // Validate MIME type
            if (file.ContentType != AllowedMimeType && file.ContentType != AllowedMimeTypeAlternate)
            {
                // Additional check: verify by file extension
                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("file", "File must be a CSV (.csv) file.");
                    return View("Index");
                }
            }

            try
            {
                // Save uploaded file to temporary location
                string tempDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "temp",
                    "imports"
                );
                Directory.CreateDirectory(tempDirectory);

                string tempFilePath = Path.Combine(
                    tempDirectory,
                    $"import_{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(file.FileName)}"
                );

                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Import from temporary file
                ImportResult result = _importService.ImportFromCsv(tempFilePath);

                // Save data after successful import
                if (result.SuccessCount > 0)
                {
                    DataService.SaveAllData();
                }

                // Clean up temporary file
                try
                {
                    System.IO.File.Delete(tempFilePath);
                }
                catch
                {
                    // Log deletion failure but don't fail the import
                }

                // Pass result to view
                return View("Result", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("file", $"Error processing file: {ex.Message}");
                return View("Index");
            }
        }
    }
}
