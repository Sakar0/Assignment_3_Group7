using Assignment2.LibraryManagement;
using Assignment2.Controllers;
using Assignment2.Controllers.Api;
using Assignment2.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Assignment2.Tests
{
    /// <summary>
    /// Integration tests for the kiosk and read-only API.
    /// Verifies complete workflow scenarios and business rule enforcement.
    /// </summary>
    public class KioskAndApiIntegrationTests
    {
        [Fact]
        public void ApiList_ReturnsPagedItems()
        {
            InitializeTestData();

            var controller = new CatalogueController();
            var result = controller.GetItems(null, null, null);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var pagedResult = Assert.IsType<PagedResult<ItemDto>>(okResult.Value);
            Assert.True(pagedResult.TotalItems > 0);
        }

        [Fact]
        public void ApiDetail_WithValidCodeReturnsItem()
        {
            InitializeTestData();

            var controller = new CatalogueController();
            var result = controller.GetItemByCode("BOOK001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var itemDto = Assert.IsType<ItemDto>(okResult.Value);
            Assert.Equal("BOOK001", itemDto.LibraryCode);
        }

        [Fact]
        public void ApiDetail_WithInvalidCodeReturns404()
        {
            var controller = new CatalogueController();
            var result = controller.GetItemByCode("NOPE999");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void ApiValidation_PageMustBePositive()
        {
            var controller = new CatalogueController();
            var result = controller.GetItems(null, null, null, page: 0);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ApiValidation_PageSizeMaximum()
        {
            var controller = new CatalogueController();
            var result = controller.GetItems(null, null, null, pageSize: 999);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ApiFilter_ByType()
        {
            InitializeTestData();

            var controller = new CatalogueController();
            var result = controller.GetItems(null, "Book", null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResult = Assert.IsType<PagedResult<ItemDto>>(okResult.Value);

            Assert.All(pagedResult.Items, item => Assert.Equal("Book", item.Type));
        }

        [Fact]
        public void ApiFilter_ByStatus()
        {
            InitializeTestData();

            var controller = new CatalogueController();
            var result = controller.GetItems(null, null, "Available");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResult = Assert.IsType<PagedResult<ItemDto>>(okResult.Value);

            Assert.All(pagedResult.Items, item => Assert.Equal("Available", item.Status));
        }

        [Fact]
        public void ApiDto_ExposesOnlyPublicFields()
        {
            InitializeTestData();

            var controller = new CatalogueController();
            var result = controller.GetItemByCode("BOOK001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var itemDto = Assert.IsType<ItemDto>(okResult.Value);

            // Should have these fields
            Assert.NotNull(itemDto.LibraryCode);
            Assert.NotNull(itemDto.Name);
            Assert.NotNull(itemDto.Status);

            // Should NOT have borrower/internal data
            var dtoProperties = itemDto.GetType().GetProperties().Select(p => p.Name).ToList();
            Assert.DoesNotContain("BorrowerId", dtoProperties);
            Assert.DoesNotContain("BorrowDate", dtoProperties);
            Assert.DoesNotContain("FineAmount", dtoProperties);
        }

        [Fact]
        public void KioskCheckout_WithValidBorrowerSucceeds()
        {
            InitializeTestData();

            var controller = new KioskController();
            SetupControllerContext(controller);
            var result = controller.Checkout("BOOK001", "BR001");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(KioskController.Index), redirectResult.ActionName);

            // Item should now be borrowed
            var item = Item.AllItems.First(i => i.LibraryCode == "BOOK001");
            Assert.Equal("Borrowed", item.Status);

            // Borrow record should exist
            var record = BorrowRecord.AllBorrowRecords.First(r => r.LibraryCode == "BOOK001");
            Assert.Equal("BR001", record.BorrowerId);
            Assert.Null(record.ReturnDate);
        }

        [Fact]
        public void KioskCheckout_WithInvalidBorrowerFails()
        {
            InitializeTestData();

            var controller = new KioskController();
            SetupControllerContext(controller);
            var result = controller.Checkout("BOOK001", "INVALID999");

            Assert.IsType<RedirectToActionResult>(result);

            // Item should remain available
            var item = Item.AllItems.First(i => i.LibraryCode == "BOOK001");
            Assert.Equal("Available", item.Status);
        }

        [Fact]
        public void KioskCheckout_UnavailableItemPreventedByGetForm()
        {
            InitializeTestData();

            // Mark item as borrowed
            var item = Item.AllItems.First(i => i.LibraryCode == "BOOK002");
            item.SetStatus("Borrowed");

            var controller = new KioskController();
            SetupControllerContext(controller);
            var result = controller.Checkout("BOOK002");

            // Should redirect with error
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void KioskReturn_WithValidCodeSucceeds()
        {
            InitializeTestData();

            // Setup: create a borrow record
            var item = Item.AllItems.First(i => i.LibraryCode == "BOOK002");
            item.SetStatus("Borrowed");

            var borrowRecord = new BorrowRecord("BOOK002", "BR001");
            BorrowRecord.AllBorrowRecords.Add(borrowRecord);

            var controller = new KioskController();
            SetupControllerContext(controller);
            var result = controller.Return("BOOK002");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(KioskController.Index), redirectResult.ActionName);

            // Item should be available
            Assert.Equal("Available", item.Status);

            // Return date should be set
            var record = BorrowRecord.AllBorrowRecords.First(r => r.LibraryCode == "BOOK002");
            Assert.NotNull(record.ReturnDate);
        }

        [Fact]
        public void KioskReturn_WithNotBorrowedItemFails()
        {
            InitializeTestData();

            var controller = new KioskController();
            SetupControllerContext(controller);
            var result = controller.Return("BOOK001"); // BOOK001 never borrowed

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void KioskCatalogue_SearchByName()
        {
            InitializeTestData();

            var controller = new KioskController();
            var result = controller.Catalogue(searchName: "Gatsby");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Item>>(viewResult.Model);

            Assert.All(model, item =>
                Assert.Contains("Gatsby", item.Name, System.StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void KioskCatalogue_FilterByType()
        {
            InitializeTestData();

            var controller = new KioskController();
            var result = controller.Catalogue(searchType: "Book");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Item>>(viewResult.Model);

            Assert.All(model, item => Assert.IsType<Book>(item));
        }

        [Fact]
        public void ReceptionServiceRule_NoBorrowIfItemNotAvailable()
        {
            InitializeTestData();

            var service = new ReceptionService();

            // Mark item as Damaged
            var item = Item.AllItems.First(i => i.LibraryCode == "MUSIC001");
            item.SetStatus("Damaged");

            // Try to borrow - should fail
            bool success = service.BorrowItem("MUSIC001", "BR001");
            Assert.False(success);

            // Item should remain Damaged
            Assert.Equal("Damaged", item.Status);
        }

        [Fact]
        public void ReceptionServiceRule_NoBorrowIfBorrowerMissing()
        {
            InitializeTestData();

            var service = new ReceptionService();

            // Try to borrow with non-existent borrower
            bool success = service.BorrowItem("BOOK001", "FAKE999");
            Assert.False(success);

            // Item should remain Available
            var item = Item.AllItems.First(i => i.LibraryCode == "BOOK001");
            Assert.Equal("Available", item.Status);
        }

        [Fact]
        public void ReceptionServiceRule_FineCalculation()
        {
            InitializeTestData();

            var service = new ReceptionService();

            // Setup: item borrowed long ago
            var book = Item.AllItems.First(i => i.LibraryCode == "BOOK001");
            book.SetStatus("Borrowed");

            var record = new BorrowRecord("BOOK001", "BR001");
            // Manually set borrow date to 20 days ago
            record.BorrowDate = System.DateTime.Now.AddDays(-20);
            BorrowRecord.AllBorrowRecords.Add(record);

            // Return the item
            bool success = service.ReturnItem("BOOK001");
            Assert.True(success);

            // Fine should be calculated: 20 days - 14 days grace = 6 days * $1 = $6
            var returned = BorrowRecord.AllBorrowRecords.First(r => r.LibraryCode == "BOOK001");
            Assert.True(returned.FineAmount > 0, "Expected a fine for overdue item");
        }

        [Fact]
        public void DataPersistence_CheckoutThenLoadPreservesBorrow()
        {
            InitializeTestData();

            // Checkout
            var receptionService = new ReceptionService();
            receptionService.BorrowItem("BOOK001", "BR001");

            // Save (simulating persistence)
            var initialBorrows = BorrowRecord.AllBorrowRecords.Count;

            // State should be preserved in memory
            var activeLoans = BorrowRecord.AllBorrowRecords
                .Where(r => !r.ReturnDate.HasValue)
                .ToList();

            Assert.Equal(1, activeLoans.Count);
            Assert.Equal("BOOK001", activeLoans[0].LibraryCode);
        }

        /// <summary>
        /// Initialize test data.
        /// </summary>
        private static void InitializeTestData()
        {
            Item.AllItems.Clear();
            Borrower.AllBorrowers.Clear();
            BorrowRecord.AllBorrowRecords.Clear();

            // Add test items
            var book1 = new Book("The Great Gatsby", "Classic novel", "BOOK001", "Available", "F. Scott Fitzgerald", "Fiction");
            var book2 = new Book("To Kill a Mockingbird", "Pulitzer winner", "BOOK002", "Available", "Harper Lee", "Fiction");
            var music1 = new Music("Abbey Road", "Beatles album", "MUSIC001", "Available", "The Beatles", 1969);
            var toy1 = new Toy("Lego Set", "Building blocks", "TOY001", "Available", "Construction", "5-12");

            Item.AllItems.Add(book1);
            Item.AllItems.Add(book2);
            Item.AllItems.Add(music1);
            Item.AllItems.Add(toy1);

            // Add test borrower
            var borrower = new Borrower("BR001", "John Doe", "555-1234", "john@example.com");
            Borrower.AllBorrowers.Add(borrower);
        }

        /// <summary>
        /// Helper: Set up TempData and ControllerContext on a controller for unit testing.
        /// </summary>
        private static void SetupControllerContext(Controller controller)
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new MockTempDataProvider());

            var controllerContext = new ControllerContext(
                new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ControllerActionDescriptor()
                )
            );

            controller.ControllerContext = controllerContext;
            controller.TempData = tempData;
        }
    }

    /// <summary>
    /// Simple mock TempData provider for unit tests.
    /// </summary>
    public class MockTempDataProvider : ITempDataProvider
    {
        private IDictionary<string, object?> _data = new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            _data = values;
        }

        public IDictionary<string, object?> LoadTempData(HttpContext context)
        {
            return _data;
        }
    }
}

