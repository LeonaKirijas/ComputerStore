using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Data;
using ComputerStore.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ComputerStore.Application.Tests.Services
{

    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryService _categoryService;
        private readonly ProductService _productService;
        private readonly AppDbContext _dbContext;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryService = new Mock<ICategoryService>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _dbContext = new AppDbContext(options);

            _productRepository = _mockProductRepository.Object;
            _categoryService = _mockCategoryService.Object;
            _productService = new ProductService(_productRepository, _categoryService, _dbContext);
        }

        private void ResetDatabase()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsAllProducts()
        {
            // Arrange
            ResetDatabase();
            var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Product 1", Description = "Description 1", Price = 10, Quantity = 5 },
            new Product { ProductId = 2, Name = "Product 2", Description = "Description 2", Price = 20, Quantity = 10 }
        };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenProductExists()
        {
            // Arrange
            ResetDatabase();
            var productId = 1;
            var expectedProduct = new Product { ProductId = productId, Name = "Test Product", Description = "Test Description", Price = 50, Quantity = 10 };
            _dbContext.Products.Add(expectedProduct);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Name, result.Name);
        }

        [Fact]
        public async Task CreateProductAsync_ThrowsArgumentNullException_IfProductIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _productService.CreateProductAsync(null));
        }

        [Fact]
        public async Task CreateProductAsync_ThrowsInvalidOperationException_WhenProductAlreadyExists()
        {
            // Arrange
            var product = new Product { Name = "Existing Product" };
            _mockProductRepository.Setup(x => x.FindByNameAsync(product.Name)).ReturnsAsync(product);

            var productService = new ProductService(_mockProductRepository.Object, _categoryService, _dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => productService.CreateProductAsync(product));
        }

        [Fact]
        public async Task CalculateDiscountAsync_ReturnsTotalDiscount()
        {
            // Arrange
            ResetDatabase();
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    Name = "Product 1",
                    Description = "Description 1", // Ensure required properties are set
                    Price = 100,
                    Quantity = 10,
                    ProductCategories = new List<ProductCategory>
                    {
                        new ProductCategory { Category = new Category { Name = "CPU", Description = "Category Description" } }
                    }
                }
            };
                    _dbContext.Products.AddRange(products);
                    await _dbContext.SaveChangesAsync();

                    var basketItems = new List<BasketItemDto>
                    {
                        new BasketItemDto { ProductId = 1, Quantity = 2 }
                    };

            // Act
            var result = await _productService.CalculateDiscountAsync(basketItems);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var discountDetail = result.First();
            Assert.Equal(1, discountDetail.ProductId);
            Assert.Equal(5, discountDetail.Discount); // 100 * 0.05
        }

    }
}