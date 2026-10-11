using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service class to manage CRUD operations for library items
    /// </summary>
    public class AdminService
    {
        private static readonly string[] AllowedStatuses = { "Available", "Borrowed", "Damaged", "Destroy" };

        /// <summary>
        /// Creates and adds a new Book to the library
        /// </summary>
        public bool CreateBook(string name, string description, string libraryCode, string status, 
                               string author, string genre)
        {
            try
            {
                if (LibraryCodeExists(libraryCode))
                {
                    Console.WriteLine($"Error: LibraryCode '{libraryCode}' already exists.");
                    return false;
                }

                Book book = new Book(name, description, libraryCode, status, author, genre);
                Item.AllItems.Add(book);
                Console.WriteLine($"✓ Book '{name}' created successfully with code '{libraryCode}'.");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error creating book: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates and adds a new Music item to the library
        /// </summary>
        public bool CreateMusic(string name, string description, string libraryCode, string status, 
                                string artist, int year)
        {
            try
            {
                if (LibraryCodeExists(libraryCode))
                {
                    Console.WriteLine($"Error: LibraryCode '{libraryCode}' already exists.");
                    return false;
                }

                Music music = new Music(name, description, libraryCode, status, artist, year);
                Item.AllItems.Add(music);
                Console.WriteLine($"✓ Music '{name}' created successfully with code '{libraryCode}'.");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error creating music: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates and adds a new Toy to the library
        /// </summary>
        public bool CreateToy(string name, string description, string libraryCode, string status, 
                              string type, string ageGroup)
        {
            try
            {
                if (LibraryCodeExists(libraryCode))
                {
                    Console.WriteLine($"Error: LibraryCode '{libraryCode}' already exists.");
                    return false;
                }

                Toy toy = new Toy(name, description, libraryCode, status, type, ageGroup);
                Item.AllItems.Add(toy);
                Console.WriteLine($"✓ Toy '{name}' created successfully with code '{libraryCode}'.");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error creating toy: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Finds an item by LibraryCode
        /// </summary>
        public Item FindByLibraryCode(string libraryCode)
        {
            var item = Item.AllItems.FirstOrDefault(i => i.LibraryCode == libraryCode);
            if (item == null)
            {
                Console.WriteLine($"No item found with LibraryCode '{libraryCode}'.");
            }
            return item;
        }

        /// <summary>
        /// Searches for items by name (partial match)
        /// </summary>
        public List<Item> SearchByName(string searchTerm)
        {
            var results = Item.AllItems
                .Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (results.Count == 0)
            {
                Console.WriteLine($"No items found matching '{searchTerm}'.");
            }

            return results;
        }

        /// <summary>
        /// Updates any property of an existing item
        /// </summary>
        public bool UpdateItem(string libraryCode, string propertyName, string newValue)
        {
            var item = FindByLibraryCode(libraryCode);
            if (item == null)
            {
                return false;
            }

            try
            {
                switch (propertyName.ToLower())
                {
                    case "name":
                        item.Name = newValue;
                        Console.WriteLine($"✓ Item name updated to '{newValue}'.");
                        return true;

                    case "description":
                        item.Description = newValue;
                        Console.WriteLine($"✓ Item description updated to '{newValue}'.");
                        return true;

                    case "status":
                        item.SetStatus(newValue);
                        Console.WriteLine($"✓ Item status updated to '{newValue}'.");
                        return true;

                    case "author":
                        if (item is Book book)
                        {
                            book.Author = newValue;
                            Console.WriteLine($"✓ Book author updated to '{newValue}'.");
                            return true;
                        }
                        Console.WriteLine("Error: Author property only applies to Book items.");
                        return false;

                    case "genre":
                        if (item is Book bookGenre)
                        {
                            bookGenre.Genre = newValue;
                            Console.WriteLine($"✓ Book genre updated to '{newValue}'.");
                            return true;
                        }
                        Console.WriteLine("Error: Genre property only applies to Book items.");
                        return false;

                    case "artist":
                        if (item is Music music)
                        {
                            music.Artist = newValue;
                            Console.WriteLine($"✓ Music artist updated to '{newValue}'.");
                            return true;
                        }
                        Console.WriteLine("Error: Artist property only applies to Music items.");
                        return false;

                    case "year":
                        if (item is Music musicYear)
                        {
                            if (int.TryParse(newValue, out int year))
                            {
                                musicYear.Year = year;
                                Console.WriteLine($"✓ Music year updated to {year}.");
                                return true;
                            }
                            Console.WriteLine("Error: Year must be a valid integer.");
                            return false;
                        }
                        Console.WriteLine("Error: Year property only applies to Music items.");
                        return false;

                    case "type":
                        if (item is Toy toy)
                        {
                            toy.Type = newValue;
                            Console.WriteLine($"✓ Toy type updated to '{newValue}'.");
                            return true;
                        }
                        Console.WriteLine("Error: Type property only applies to Toy items.");
                        return false;

                    case "agegroup":
                        if (item is Toy toyAge)
                        {
                            toyAge.AgeGroup = newValue;
                            Console.WriteLine($"✓ Toy age group updated to '{newValue}'.");
                            return true;
                        }
                        Console.WriteLine("Error: AgeGroup property only applies to Toy items.");
                        return false;

                    default:
                        Console.WriteLine($"Error: Unknown property '{propertyName}'.");
                        return false;
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error updating item: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes an item by LibraryCode
        /// </summary>
        public bool DeleteByLibraryCode(string libraryCode)
        {
            var item = FindByLibraryCode(libraryCode);
            if (item == null)
            {
                return false;
            }

            Item.AllItems.Remove(item);
            Console.WriteLine($"✓ Item with LibraryCode '{libraryCode}' deleted successfully.");
            return true;
        }

        /// <summary>
        /// Displays all items in the library
        /// </summary>
        public void DisplayAllItems()
        {
            if (Item.AllItems.Count == 0)
            {
                Console.WriteLine("No items in the library.");
                return;
            }

            Console.WriteLine("\n--- All Items in Library ---");
            for (int i = 0; i < Item.AllItems.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Item.AllItems[i]}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Checks if a LibraryCode already exists
        /// </summary>
        private bool LibraryCodeExists(string libraryCode)
        {
            return Item.AllItems.Any(i => i.LibraryCode == libraryCode);
        }

        /// <summary>
        /// Displays all allowed status values
        /// </summary>
        public void DisplayAllowedStatuses()
        {
            Console.WriteLine("Allowed Status values: " + string.Join(", ", AllowedStatuses));
        }
    }
}
