using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Service for importing catalogue items from CSV files
    /// </summary>
    public class CatalogueImportService
    {
        private static readonly string[] AllowedStatuses = { "Available", "Borrowed", "Damaged", "Destroy" };

        /// <summary>
        /// Imports items from a CSV file
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>ImportResult with success/failure details</returns>
        public ImportResult ImportFromCsv(string filePath)
        {
            var result = new ImportResult();

            if (!File.Exists(filePath))
            {
                result.FailureCount = 1;
                result.FailedRows.Add(new ImportError
                {
                    RowNumber = 0,
                    RawContent = string.Empty,
                    ErrorMessage = "File not found."
                });
                return result;
            }

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string headerLine = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(headerLine))
                    {
                        result.FailureCount = 1;
                        result.FailedRows.Add(new ImportError
                        {
                            RowNumber = 1,
                            RawContent = string.Empty,
                            ErrorMessage = "CSV file is empty or has no header row."
                        });
                        return result;
                    }

                    // Parse header
                    var headers = ParseCsvLine(headerLine);
                    var columnIndices = MapColumnIndices(headers);

                    if (!ValidateHeaderColumns(columnIndices, out string headerError))
                    {
                        result.FailureCount = 1;
                        result.FailedRows.Add(new ImportError
                        {
                            RowNumber = 1,
                            RawContent = headerLine,
                            ErrorMessage = headerError
                        });
                        return result;
                    }

                    int rowNumber = 2;
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            // Skip blank rows silently
                            rowNumber++;
                            continue;
                        }

                        var columns = ParseCsvLine(line);
                        var importError = ProcessRow(columns, columnIndices, rowNumber);

                        if (importError == null)
                        {
                            result.SuccessCount++;
                        }
                        else
                        {
                            result.FailureCount++;
                            result.FailedRows.Add(importError);
                        }

                        rowNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.FailedRows.Add(new ImportError
                {
                    RowNumber = 0,
                    RawContent = string.Empty,
                    ErrorMessage = $"Error reading file: {ex.Message}"
                });
            }

            return result;
        }

        /// <summary>
        /// Parses a CSV line handling quoted fields and escaped quotes
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool insideQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote mode
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (c == ',' && !insideQuotes)
                {
                    // Field separator
                    fields.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            fields.Add(currentField.ToString().Trim());
            return fields;
        }

        /// <summary>
        /// Maps column names to their indices
        /// </summary>
        private Dictionary<string, int> MapColumnIndices(List<string> headers)
        {
            var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Count; i++)
            {
                string header = headers[i].Trim().ToLower();
                if (!string.IsNullOrEmpty(header))
                {
                    indices[header] = i;
                }
            }

            return indices;
        }

        /// <summary>
        /// Validates that required columns are present
        /// </summary>
        private bool ValidateHeaderColumns(Dictionary<string, int> columnIndices, out string error)
        {
            error = string.Empty;

            if (!columnIndices.ContainsKey("type"))
            {
                error = "Missing required column: Type";
                return false;
            }

            if (!columnIndices.ContainsKey("name"))
            {
                error = "Missing required column: Name";
                return false;
            }

            if (!columnIndices.ContainsKey("librarycode"))
            {
                error = "Missing required column: LibraryCode";
                return false;
            }

            if (!columnIndices.ContainsKey("status"))
            {
                error = "Missing required column: Status";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes a single CSV row and creates an Item if valid
        /// </summary>
        private ImportError ProcessRow(List<string> columns, Dictionary<string, int> columnIndices, int rowNumber)
        {
            try
            {
                // Extract common fields
                string type = GetColumnValue(columns, columnIndices, "type");
                string name = GetColumnValue(columns, columnIndices, "name");
                string description = GetColumnValue(columns, columnIndices, "description");
                string libraryCode = GetColumnValue(columns, columnIndices, "librarycode");
                string status = GetColumnValue(columns, columnIndices, "status");

                // Validate common fields
                if (string.IsNullOrWhiteSpace(type))
                    return CreateError(rowNumber, columns, "Type is required");

                if (string.IsNullOrWhiteSpace(name))
                    return CreateError(rowNumber, columns, "Name is required");

                if (string.IsNullOrWhiteSpace(libraryCode))
                    return CreateError(rowNumber, columns, "LibraryCode is required");

                if (string.IsNullOrWhiteSpace(status))
                    return CreateError(rowNumber, columns, "Status is required");

                // Validate status value
                if (!AllowedStatuses.Contains(status))
                    return CreateError(rowNumber, columns, $"Invalid status '{status}'. Must be one of: {string.Join(", ", AllowedStatuses)}");

                // Check for duplicate library code
                if (Item.AllItems.Any(i => i.LibraryCode == libraryCode))
                    return CreateError(rowNumber, columns, $"LibraryCode '{libraryCode}' already exists");

                // Process by type
                Item newItem = null;

                if (type.Equals("Book", StringComparison.OrdinalIgnoreCase))
                {
                    string author = GetColumnValue(columns, columnIndices, "author");
                    string genre = GetColumnValue(columns, columnIndices, "genre");

                    if (string.IsNullOrWhiteSpace(author))
                        return CreateError(rowNumber, columns, "Book requires Author");
                    if (string.IsNullOrWhiteSpace(genre))
                        return CreateError(rowNumber, columns, "Book requires Genre");

                    newItem = new Book(name, description, libraryCode, status, author, genre);
                }
                else if (type.Equals("Music", StringComparison.OrdinalIgnoreCase))
                {
                    string artist = GetColumnValue(columns, columnIndices, "artist");
                    string yearStr = GetColumnValue(columns, columnIndices, "year");

                    if (string.IsNullOrWhiteSpace(artist))
                        return CreateError(rowNumber, columns, "Music requires Artist");

                    if (!int.TryParse(yearStr, out int year))
                        return CreateError(rowNumber, columns, "Music Year must be a valid integer");

                    newItem = new Music(name, description, libraryCode, status, artist, year);
                }
                else if (type.Equals("Toy", StringComparison.OrdinalIgnoreCase))
                {
                    string toyType = GetColumnValue(columns, columnIndices, "toytype")
                                  ?? GetColumnValue(columns, columnIndices, "type");
                    string ageGroup = GetColumnValue(columns, columnIndices, "agegroup");

                    if (string.IsNullOrWhiteSpace(toyType) || toyType.Equals(type, StringComparison.OrdinalIgnoreCase))
                        return CreateError(rowNumber, columns, "Toy requires ToyType (separate from Type column)");
                    if (string.IsNullOrWhiteSpace(ageGroup))
                        return CreateError(rowNumber, columns, "Toy requires AgeGroup");

                    newItem = new Toy(name, description, libraryCode, status, toyType, ageGroup);
                }
                else
                {
                    return CreateError(rowNumber, columns, $"Invalid type '{type}'. Must be Book, Music, or Toy");
                }

                // If creation succeeded, add to collection
                if (newItem != null)
                {
                    Item.AllItems.Add(newItem);
                    return null; // Success
                }

                return CreateError(rowNumber, columns, "Failed to create item");
            }
            catch (ArgumentException ex)
            {
                return CreateError(rowNumber, columns, $"Validation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CreateError(rowNumber, columns, $"Error processing row: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a column value by name, returns empty string if not found
        /// </summary>
        private string GetColumnValue(List<string> columns, Dictionary<string, int> columnIndices, string columnName)
        {
            if (columnIndices.TryGetValue(columnName, out int index) && index < columns.Count)
            {
                return columns[index].Trim();
            }
            return string.Empty;
        }

        /// <summary>
        /// Creates an ImportError with raw content
        /// </summary>
        private ImportError CreateError(int rowNumber, List<string> columns, string message)
        {
            return new ImportError
            {
                RowNumber = rowNumber,
                RawContent = string.Join(", ", columns),
                ErrorMessage = message
            };
        }
    }

    }
