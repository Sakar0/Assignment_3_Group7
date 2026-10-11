using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Borrower class - represents a person who can borrow items from the library
    /// Stores contact information and maintains a shared list of all borrowers
    /// </summary>
    public class Borrower
    {
        public string BorrowerId { get; set; }   // Unique ID (like "STU001" for student 1)
        public string Name { get; set; }         // Person's full name
        public string Phone { get; set; }        // Contact phone number
        public string Email { get; set; }        // Contact email address

        // Shared list - stores ALL borrowers in the system
        public static List<Borrower> AllBorrowers { get; } = new List<Borrower>();

        /// <summary>
        /// Create a new borrower
        /// BorrowerId and Name are required - throws error if empty
        /// Phone and Email are optional
        /// </summary>
        public Borrower(string borrowerId, string name, string phone, string email)
        {
            // Make sure BorrowerId is not empty
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("BorrowerId cannot be empty.");

            // Make sure Name is not empty
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");

            BorrowerId = borrowerId;
            Name = name;
            // If phone/email not provided, use empty string instead of null
            Phone = phone ?? "";
            Email = email ?? "";
        }

        /// <summary>
        /// Display borrower information as text
        /// </summary>
        public override string ToString()
        {
            return $"BorrowerId: {BorrowerId}, Name: {Name}, Phone: {Phone}, Email: {Email}";
        }
    }
}
