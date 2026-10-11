namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Book class - a type of item in the library
    /// Inherits from Item (gets Name, Description, LibraryCode, Status)
    /// Adds book-specific properties: Author and Genre
    /// </summary>
    public class Book : Item
    {
        public string Author { get; set; }    // Who wrote the book
        public string Genre { get; set; }     // Type of book (Fiction, Science Fiction, Mystery, etc.)

        /// <summary>
        /// Create a new book
        /// Parameters are passed to the Item base class, then Author and Genre are set
        /// </summary>
        public Book(string name, string description, string libraryCode, string status, 
                    string author, string genre)
            : base(name, description, libraryCode, status)
        {
            Author = author;
            Genre = genre;
        }

        /// <summary>
        /// Display all book information as text
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + $", Type: Book, Author: {Author}, Genre: {Genre}";
        }
    }
}
