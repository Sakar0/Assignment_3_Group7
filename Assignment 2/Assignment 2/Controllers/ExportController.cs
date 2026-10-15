using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using Assignment2.Services;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Controller for exporting manager reports to CSV and PDF formats
    /// </summary>
    public class ExportController : Controller
    {
        private readonly ReportExportService _exportService = new ReportExportService();

        /// <summary>
        /// GET /Export/Index - Main export options page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET /Export/DownloadCsv - Downloads comprehensive analytics report as CSV
        /// </summary>
        public IActionResult DownloadCsv()
        {
            var csv = _exportService.GenerateCsvReport();
            var fileName = $"Library_Analytics_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv);

            return File(bytes, "text/csv", fileName);
        }

        /// <summary>
        /// GET /Export/DownloadItemBreakdownCsv - Downloads item type breakdown as CSV
        /// </summary>
        public IActionResult DownloadItemBreakdownCsv()
        {
            var csv = _exportService.GenerateItemBreakdownCsv();
            var fileName = $"Library_ItemBreakdown_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv);

            return File(bytes, "text/csv", fileName);
        }

        /// <summary>
        /// GET /Export/ViewTextReport - Displays text-formatted report for viewing
        /// </summary>
        public IActionResult ViewTextReport()
        {
            var report = _exportService.GenerateTextReport();
            return Ok(report);
        }

        /// <summary>
        /// GET /Export/DownloadTextReport - Downloads text report as .txt file
        /// </summary>
        public IActionResult DownloadTextReport()
        {
            var report = _exportService.GenerateTextReport();
            var fileName = $"Library_Report_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";
            var bytes = Encoding.UTF8.GetBytes(report);

            return File(bytes, "text/plain", fileName);
        }

        /// <summary>
        /// GET /Export/DownloadPdf - Generates and downloads report as PDF
        /// NOTE: For full PDF generation, requires iTextSharp or PdfSharp NuGet packages
        /// Current implementation provides text as plain text for manual PDF conversion
        /// </summary>
        public IActionResult DownloadPdf()
        {
            var report = _exportService.GenerateTextReport();
            var fileName = $"Library_Report_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";
            var bytes = Encoding.UTF8.GetBytes(report);

            // Returns as text/plain; in production, would convert to PDF via iTextSharp/PdfSharp
            // For now, return as text that can be saved/printed as PDF
            return File(bytes, "text/plain", fileName);
        }
    }
}
