using System;
using System.IO;
using System.Linq;
using Assignment2.LibraryManagement;

namespace Assignment2.Tests
{
    /// <summary>
    /// Manual test suite for CatalogueImportService
    /// Run with: dotnet run -- test
    /// </summary>
    public class ImportServiceTests
    {
        private static string ResolveFixture(string fileName)
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Assignment 2.csproj")))
            {
                directory = directory.Parent;
            }

            if (directory != null)
            {
                foreach (string folder in new[] { directory.FullName, Path.Combine(directory.FullName, "Assignment 2") })
                {
                    string candidate = Path.Combine(folder, fileName);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            throw new FileNotFoundException($"Test fixture '{fileName}' not found from {AppDomain.CurrentDomain.BaseDirectory}.");
        }

        public static void RunTests()
        {
            Console.WriteLine("\n=== Catalogue Import Service Test Suite ===\n");

            // Test 1: Valid sample file
            Console.WriteLine("Test 1: Valid sample file with mixed item types");
            TestSampleFile();

            // Test 2: Empty file
            Console.WriteLine("\nTest 2: Empty file (header only)");
            TestEmptyFile();

            // Test 3: Missing required fields
            Console.WriteLine("\nTest 3: Missing required fields");
            TestMissingFields();

            // Test 4: Invalid status value
            Console.WriteLine("\nTest 4: Invalid status value");
            TestInvalidStatus();

            // Test 5: Invalid year (Music)
            Console.WriteLine("\nTest 5: Invalid year for Music item");
            TestInvalidYear();

            // Test 6: Duplicate library code
            Console.WriteLine("\nTest 6: Duplicate library code detection");
            TestDuplicateCode();

            // Test 7: Mixed valid and blank rows
            Console.WriteLine("\nTest 7: Mixed valid and blank rows");
            TestMixedValidBlank();

            Console.WriteLine("\n=== Test Suite Complete ===\n");
        }

        private static void TestSampleFile()
        {
            var service = new CatalogueImportService();
            string filePath = ResolveFixture("sample_import.csv");
            int initialCount = Item.AllItems.Count;
            var result = service.ImportFromCsv(filePath);

            Console.WriteLine($"  Initial items: {initialCount}");
            Console.WriteLine($"  ✓ Successes: {result.SuccessCount}");
            Console.WriteLine($"  ✗ Failures: {result.FailureCount}");
            Console.WriteLine($"  Final items: {Item.AllItems.Count}");

            if (result.FailureCount > 0)
            {
                foreach (var error in result.FailedRows)
                {
                    Console.WriteLine($"    Row {error.RowNumber}: {error.ErrorMessage}");
                }
            }
        }

        private static void TestEmptyFile()
        {
            var service = new CatalogueImportService();
            string filePath = ResolveFixture("test_empty.csv");
            var result = service.ImportFromCsv(filePath);
            Console.WriteLine($"  ✓ Successes: {result.SuccessCount}");
            Console.WriteLine($"  ✗ Failures: {result.FailureCount}");
            Console.WriteLine($"  Expected: 0 successes, 0+ failures");

            if (result.FailureCount == 0)
            {
                Console.WriteLine("  [PASS] Empty file handled correctly");
            }
            else
            {
                Console.WriteLine($"  [INFO] Failures: {string.Join("; ", result.FailedRows.Select(e => e.ErrorMessage))}");
            }
        }

        private static void TestMissingFields()
        {
            var service = new CatalogueImportService();
            string filePath = ResolveFixture("test_missing_fields.csv");
            var result = service.ImportFromCsv(filePath);
            Console.WriteLine($"  ✓ Successes: {result.SuccessCount}");
            Console.WriteLine($"  ✗ Failures: {result.FailureCount}");

            if (result.FailureCount > 0)
            {
                Console.WriteLine($"  [PASS] Missing fields detected: {result.FailedRows[0].ErrorMessage}");
            }
            else
            {
                Console.WriteLine("  [FAIL] Missing fields should have caused an error");
            }
        }

        private static void TestInvalidStatus()
        {
            var service = new CatalogueImportService();
            string filePath = ResolveFixture("test_invalid_status.csv");
            var result = service.ImportFromCsv(filePath);
            Console.WriteLine($"  ✓ Successes: {result.SuccessCount}");
            Console.WriteLine($"  ✗ Failures: {result.FailureCount}");

            if (result.FailureCount > 0 && result.FailedRows[0].ErrorMessage.Contains("status", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  [PASS] Invalid status detected: {result.FailedRows[0].ErrorMessage}");
            }
        }

        private static void TestInvalidYear()
        {
            var service = new CatalogueImportService();
            string filePath = ResolveFixture("test_invalid_year.csv");
            var result = service.ImportFromCsv(filePath);
            Console.WriteLine($"  ✓ Successes: {result.SuccessCount}");
            Console.WriteLine($"  ✗ Failures: {result.FailureCount}");

            if (result.FailureCount > 0 && result.FailedRows[0].ErrorMessage.Contains("Year", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  [PASS] Invalid year detected: {result.FailedRows[0].ErrorMessage}");
            }
        }

        private static void TestDuplicateCode()
        {
            var service = new CatalogueImportService();
            string filePath = ResolveFixture("test_duplicate_code.csv");
            var result = service.ImportFromCsv(filePath);
            Console.WriteLine($"  ✓ Successes: {result.SuccessCount}");
            Console.WriteLine($"  ✗ Failures: {result.FailureCount}");

            if (result.FailureCount > 0 && result.FailedRows[0].ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  [PASS] Duplicate code detected: {result.FailedRows[0].ErrorMessage}");
            }
        }

        private static void TestMixedValidBlank()
        {
            var service = new CatalogueImportService();
            string filePath = ResolveFixture("test_mixed_valid_blank.csv");
            int initialCount = Item.AllItems.Count;
            var result = service.ImportFromCsv(filePath);

            Console.WriteLine($"  Initial items: {initialCount}");
            Console.WriteLine($"  ✓ Successes: {result.SuccessCount}");
            Console.WriteLine($"  ✗ Failures: {result.FailureCount}");
            Console.WriteLine($"  Final items: {Item.AllItems.Count}");

            if (result.SuccessCount >= 3)
            {
                Console.WriteLine("  [PASS] Valid rows imported successfully, blank rows skipped");
            }
        }
    }
}
