using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ComputerStore.Application.Tests.Services
{
    public class CategoryServiceTests
    {
        private AppDbContext _dbContext;
        private CategoryService _categoryService;

        private void SetupInMemoryDatabase()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase-{System.Guid.NewGuid()}")
                .Options;
            _dbContext = new AppDbContext(options);
            _categoryService = new CategoryService(_dbContext);
        }

        [Fact]
        public async Task FindByNameAsync_ReturnsCategory_WhenCategoryExists()
        {
            // Arrange
            SetupInMemoryDatabase();
            var categoryName = "Test Category";
            var expectedCategory = new Category { Name = categoryName, Description = "Description" };
            _dbContext.Categories.Add(expectedCategory);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _categoryService.FindByNameAsync(categoryName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCategory.Name, result.Name);
        }

        [Fact]
        public async Task CreateCategoryAsync_ReturnsCategory_WhenCategoryIsCreated()
        {
            // Arrange
            SetupInMemoryDatabase();
            var category = new Category { Name = "New Category", Description = "Description" };

            // Act
            var result = await _categoryService.CreateCategoryAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category.Name, result.Name);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            // Arrange
            SetupInMemoryDatabase();
            var categories = new List<Category>
            {
                new Category { Name = "Category 1", Description = "Description 1" },
                new Category { Name = "Category 2", Description = "Description 2" }
            };
            _dbContext.Categories.AddRange(categories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}