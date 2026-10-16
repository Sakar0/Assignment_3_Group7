using Assignment2.LibraryManagement;
using Assignment2.Models.Api;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Assignment2.Controllers.Api
{
    /// <summary>
    /// Read-only REST API for the public library catalogue.
    /// GET-only by design: no action in this controller can create, update or
    /// delete data, and only read paths (SearchService / Item lookups) are used.
    /// Responses use DTOs so internal entities and borrower data are never exposed.
    /// </summary>
    [ApiController]
    [Route("api/catalogue")]
    [Produces("application/json")]
    public class CatalogueController : ControllerBase
    {
        private const int DefaultPageSize = 12;
        private const int MaxPageSize = 50;
        private const int MaxLibraryCodeLength = 20;

        private static readonly string[] AllowedTypes = { "Book", "Music", "Toy" };
        private static readonly string[] AllowedStatuses = { "Available", "Borrowed", "Damaged", "Destroy" };

        /// <summary>
        /// GET /api/catalogue/docs
        /// Returns a self-describing machine-readable summary of the API surface
        /// (endpoints, parameters, limits and error codes). Read-only.
        /// </summary>
        [HttpGet("docs")]
        public IActionResult GetDocs()
        {
            var docs = new
            {
                title = "Library Catalogue API (read-only)",
                version = "1.0",
                baseRoute = "/api/catalogue",
                writeOperationsAllowed = false,
                endpoints = new object[]
                {
                    new
                    {
                        method = "GET",
                        path = "/api/catalogue",
                        description = "Paged list of catalogue items.",
                        queryParameters = new object[]
                        {
                            new { name = "name", type = "string", required = false, notes = "Partial, case-insensitive name filter." },
                            new { name = "type", type = "string", required = false, notes = "One of: Book, Music, Toy." },
                            new { name = "status", type = "string", required = false, notes = "One of: Available, Borrowed, Damaged, Destroy." },
                            new { name = "page", type = "int", required = false, notes = "Page number >= 1. Default 1." },
                            new { name = "pageSize", type = "int", required = false, notes = $"1-{MaxPageSize}. Default {DefaultPageSize}." }
                        },
                        successStatus = 200
                    },
                    new
                    {
                        method = "GET",
                        path = "/api/catalogue/{libraryCode}",
                        description = "Single catalogue item by library code.",
                        routeParameters = new object[]
                        {
                            new { name = "libraryCode", type = "string", required = true, notes = $"Non-empty, at most {MaxLibraryCodeLength} characters." }
                        },
                        successStatus = 200,
                        errorStatuses = new[] { 400, 404 }
                    }
                },
                errors = new
                {
                    format = "RFC 7807 ProblemDetails (application/json)",
                    statuses = new object[]
                    {
                        new { status = 400, meaning = "Invalid query or route parameter." },
                        new { status = 404, meaning = "Requested item does not exist." },
                        new { status = 405, meaning = "Method not allowed - the API is read-only (GET only)." },
                        new { status = 500, meaning = "Unexpected server error (no internal details are exposed)." }
                    }
                }
            };

            return Ok(docs);
        }

        /// <summary>
        /// GET /api/catalogue
        /// Lists catalogue items with optional filtering and pagination.
        /// </summary>
        /// <param name="name">Optional partial, case-insensitive name filter.</param>
        /// <param name="type">Optional type filter: Book, Music or Toy.</param>
        /// <param name="status">Optional status filter: Available, Borrowed, Damaged or Destroy.</param>
        /// <param name="page">Page number, starting at 1 (default 1).</param>
        /// <param name="pageSize">Items per page, 1-50 (default 12).</param>
        [HttpGet]
        public IActionResult GetItems(
            [FromQuery] string? name,
            [FromQuery] string? type,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                if (page < 1)
                    return InvalidParameter("'page' must be 1 or greater.");

                if (pageSize < 1 || pageSize > MaxPageSize)
                    return InvalidParameter($"'pageSize' must be between 1 and {MaxPageSize}.");

                if (name != null && name.Length > 100)
                    return InvalidParameter("'name' must be 100 characters or fewer.");

                if (!string.IsNullOrWhiteSpace(type) &&
                    !AllowedTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                    return InvalidParameter("'type' must be one of: Book, Music, Toy.");

                if (!string.IsNullOrWhiteSpace(status) &&
                    !AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                    return InvalidParameter("'status' must be one of: Available, Borrowed, Damaged, Destroy.");

                // Reuse the existing search service (works on an in-memory snapshot).
                var matches = SearchService.SearchItems(name ?? "", "", type ?? "");

                if (!string.IsNullOrWhiteSpace(status))
                {
                    matches = matches
                        .Where(i => i.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                int totalItems = matches.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var pageItems = matches
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ItemDto.FromItem)
                    .ToList();

                return Ok(new PagedResult<ItemDto>
                {
                    Items = pageItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                // Log internally, but never leak exception details to API consumers.
                Console.WriteLine($"API catalogue list error: {ex.Message}");
                return Problem(
                    title: "An unexpected error occurred.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// GET /api/catalogue/{libraryCode}
        /// Retrieves a single catalogue item by its library code (e.g. BOOK001).
        /// </summary>
        [HttpGet("{libraryCode}")]
        public IActionResult GetItemByCode(string libraryCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(libraryCode) ||
                    libraryCode.Trim().Length > MaxLibraryCodeLength)
                {
                    return InvalidParameter(
                        $"'libraryCode' must be a non-empty code of at most {MaxLibraryCodeLength} characters.");
                }

                var item = SearchService.FindByLibraryCode(libraryCode.Trim());
                if (item == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Item not found.",
                        Detail = $"No catalogue item exists with library code '{libraryCode.Trim()}'.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(ItemDto.FromItem(item));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API catalogue detail error: {ex.Message}");
                return Problem(
                    title: "An unexpected error occurred.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Builds a consistent RFC 7807 400 response for invalid query/route parameters.
        /// </summary>
        private BadRequestObjectResult InvalidParameter(string detail)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request parameter.",
                Detail = detail,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
