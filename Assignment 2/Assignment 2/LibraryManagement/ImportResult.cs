using System;
using System.Collections.Generic;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Represents the result of a catalogue import operation
    /// </summary>
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<Item> ImportedItems { get; set; } = new List<Item>();
        public List<ImportError> FailedRows { get; set; } = new List<ImportError>();
    }

    /// <summary>
    /// Represents a single failed import row
    /// </summary>
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string RawContent { get; set; }
        public string ErrorMessage { get; set; }
    }
}
