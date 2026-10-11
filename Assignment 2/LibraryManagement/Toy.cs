namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Toy class - a type of item in the library
    /// Inherits from Item (gets Name, Description, LibraryCode, Status)
    /// Adds toy-specific properties: Type and AgeGroup
    /// </summary>
    public class Toy : Item
    {
        public string Type { get; set; }        // What kind of toy (Building Blocks, Puzzle, Action Figure, etc.)
        public string AgeGroup { get; set; }    // Recommended age range (example: "3-8" or "5-12")

        /// <summary>
        /// Create a new toy item
        /// Parameters are passed to the Item base class, then Type and AgeGroup are set
        /// </summary>
        public Toy(string name, string description, string libraryCode, string status, 
                   string type, string ageGroup)
            : base(name, description, libraryCode, status)
        {
            Type = type;
            AgeGroup = ageGroup;
        }

        /// <summary>
        /// Display all toy information as text
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + $", Type: Toy, ToyType: {Type}, AgeGroup: {AgeGroup}";
        }
    }
}
