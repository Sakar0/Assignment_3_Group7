namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Music class - a type of item in the library
    /// Inherits from Item (gets Name, Description, LibraryCode, Status)
    /// Adds music-specific properties: Artist and Year
    /// </summary>
    public class Music : Item
    {
        public string Artist { get; set; }  // Who performs the music (singer, band, etc.)
        public int Year { get; set; }       // What year the music was released

        /// <summary>
        /// Create a new music item
        /// Parameters are passed to the Item base class, then Artist and Year are set
        /// </summary>
        public Music(string name, string description, string libraryCode, string status, 
                     string artist, int year)
            : base(name, description, libraryCode, status)
        {
            Artist = artist;
            Year = year;
        }

        /// <summary>
        /// Display all music information as text
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + $", Type: Music, Artist: {Artist}, Year: {Year}";
        }
    }
}
