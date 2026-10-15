using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Assignment2.LibraryManagement;

namespace Assignment2.Controllers
{
    /// <summary>
    /// Controller for manager analytics and reporting
    /// Provides dashboards and reports using ManagerService calculations
    /// </summary>
    public class AnalyticsController : Controller
    {
        private readonly ManagerService _managerService = new ManagerService();

        /// <summary>
        /// Displays the main analytics dashboard
        /// GET: /Analytics/Dashboard
        /// </summary>
        public IActionResult Dashboard()
        {
            var statistics = _managerService.GetComprehensiveStatistics();
            return View(statistics);
        }

        /// <summary>
        /// Displays detailed borrowing analytics
        /// GET: /Analytics/Borrowing
        /// </summary>
        public IActionResult Borrowing()
        {
            var statistics = _managerService.GetBorrowingStatistics();
            return View(statistics);
        }

        /// <summary>
        /// Displays detailed item status analytics
        /// GET: /Analytics/ItemStatus
        /// </summary>
        public IActionResult ItemStatus()
        {
            var statistics = _managerService.GetItemStatusStatistics();

            // Calculate breakdown by type
            var typeBreakdown = Item.AllItems
                .GroupBy(i => i.GetType().Name)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderBy(x => x.Type)
                .ToList();

            ViewBag.TypeBreakdown = typeBreakdown;
            return View(statistics);
        }

        /// <summary>
        /// Displays detailed fine analytics
        /// GET: /Analytics/Fines
        /// </summary>
        public IActionResult Fines()
        {
            var statistics = _managerService.GetFineStatistics();
            return View(statistics);
        }

        /// <summary>
        /// Displays analytics by item type with detailed breakdowns
        /// GET: /Analytics/ByType
        /// </summary>
        public IActionResult ByType()
        {
            var stats = new Dictionary<string, dynamic>();

            // Books by Genre
            var booksByGenre = Item.AllItems
                .OfType<Book>()
                .GroupBy(b => b.Genre)
                .Select(g => new { Genre = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Music by Artist
            var musicByArtist = Item.AllItems
                .OfType<Music>()
                .GroupBy(m => m.Artist)
                .Select(g => new { Artist = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Toys by Age Group
            var toysByAgeGroup = Item.AllItems
                .OfType<Toy>()
                .GroupBy(t => t.AgeGroup)
                .Select(g => new { AgeGroup = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.BooksByGenre = booksByGenre;
            ViewBag.MusicByArtist = musicByArtist;
            ViewBag.ToysByAgeGroup = toysByAgeGroup;

            return View();
        }
    }
}
