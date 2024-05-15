using ComputerStore.Application.Interfaces;
using ComputerStore.DTOs;
using ComputerStore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ComputerStore.Infrastructure.Data;

namespace ComputerStore.Application.Tests.Controller
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly ProductsController _productsController;
        private readonly AppDbContext _dbContext;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _dbContext = new AppDbContext(options);

            _productsController = new ProductsController(_mockProductService.Object, _mockCategoryService.Object, _dbContext);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Product 1" },
            new Product { ProductId = 2, Name = "Product 2" }
        };
            _mockProductService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await _productsController.GetAllProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnProducts = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Equal(2, returnProducts.Count);
        }

        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 1;
            _mockProductService.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync((Product)null);

            // Act
            var result = await _productsController.GetProductById(productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"No product found with ID {productId}.", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateProduct_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _productsController.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _productsController.CreateProduct(new ProductCreateDto());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsNoContent_WhenProductIsUpdated()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product
            {
                ProductId = productId,
                Name = "Product 1",
                Description = "Description",
                Price = 10,
                Quantity = 5,
                ProductCategories = new List<ProductCategory>
        {
            new ProductCategory { Category = new Category { Name = "Category 1" } }
        }
            };
            var updatedProductDto = new ProductCreateDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 20,
                Quantity = 10,
                Categories = new List<string> { "Category 1" }
            };

            _mockProductService.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync(existingProduct);
            _mockCategoryService.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new Category { Name = "Category 1" });

            // Act
            var result = await _productsController.UpdateProduct(productId, updatedProductDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductIsDeleted()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product { ProductId = productId, Name = "Product 1" };
            _mockProductService.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _productsController.DeleteProduct(productId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CalculateDiscount_ReturnsTotalDiscount()
        {
            // Arrange
            var basketItems = new List<BasketItemDto>
        {
            new BasketItemDto { ProductId = 1, Quantity = 2 }
        };
            var expectedDiscount = 10m;
            _mockProductService.Setup(x => x.CalculateDiscountAsync(basketItems)).ReturnsAsync(expectedDiscount);

            // Act
            var result = await _productsController.CalculateDiscount(basketItems);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value.GetType().GetProperty("totalDiscount").GetValue(okResult.Value);
            Assert.Equal(expectedDiscount, value);
        }
    }
}