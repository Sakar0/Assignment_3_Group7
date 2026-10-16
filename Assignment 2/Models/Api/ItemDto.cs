using Assignment2.LibraryManagement;

namespace Assignment2.Models.Api
{
    /// <summary>
    /// Read-only representation of a library item for the public catalogue API.
    /// Exposes only the fields API consumers need - internal entities, borrower
    /// data and persistence details are never returned.
    /// Properties serialize as camelCase JSON by ASP.NET Core defaults.
    /// Type-specific properties are null when they do not apply to the item type.
    /// </summary>
    public class ItemDto
    {
        public string LibraryCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsAvailable { get; set; }

        // Book-specific (null for other types)
        public string? Author { get; set; }
        public string? Genre { get; set; }

        // Music-specific (null for other types)
        public string? Artist { get; set; }
        public int? Year { get; set; }

        // Toy-specific (null for other types)
        public string? ToyType { get; set; }
        public string? AgeGroup { get; set; }

        /// <summary>
        /// Maps an internal Item entity to its public read-only DTO form.
        /// </summary>
        public static ItemDto FromItem(Item item)
        {
            var dto = new ItemDto
            {
                LibraryCode = item.LibraryCode,
                Name = item.Name,
                Description = item.Description,
                Status = item.Status,
                IsAvailable = item.Status == "Available"
            };

            switch (item)
            {
                case Book book:
                    dto.Type = "Book";
                    dto.Author = book.Author;
                    dto.Genre = book.Genre;
                    break;
                case Music music:
                    dto.Type = "Music";
                    dto.Artist = music.Artist;
                    dto.Year = music.Year;
                    break;
                case Toy toy:
                    dto.Type = "Toy";
                    dto.ToyType = toy.Type;
                    dto.AgeGroup = toy.AgeGroup;
                    break;
                default:
                    dto.Type = "Unknown";
                    break;
            }

            return dto;
        }
    }
}
