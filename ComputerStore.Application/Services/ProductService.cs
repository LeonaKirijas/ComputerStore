using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ComputerStore.DTOs;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryService _categoryService;
    private readonly AppDbContext _dbContext;

    public ProductService(IProductRepository productRepository, ICategoryService categoryService, AppDbContext dbContext)
    {
        _productRepository = productRepository;
        _categoryService = categoryService;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _dbContext.Products
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int productId)
    {
        return await _dbContext.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<List<Product>> GetProductsByIdsAsync(List<int> productId)
    {
        return await _dbContext.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => productId.Contains(p.ProductId))
            .ToListAsync();
    }

    public async Task CreateProductAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var existingProduct = await _productRepository.FindByNameAsync(product.Name);
        if (existingProduct != null)
            throw new InvalidOperationException("Product with the same name already exists.");

        // Ensure categories exist and assign to product
        foreach (var productCategory in product.ProductCategories)
        {
            var existingCategory = await _categoryService.FindByNameAsync(productCategory.Category.Name);
            if (existingCategory != null)
            {
                productCategory.CategoryId = existingCategory.CategoryId;
                productCategory.Category = existingCategory;
            }
        }

        await _productRepository.AddAsync(product);
    }

    public async Task UpdateProductAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        // Set default description if not provided
        if (string.IsNullOrEmpty(product.Description))
        {
            product.Description = "Default description"; // Set the default description here
        }

        await _productRepository.UpdateAsync(product);
    }

    public async Task<Product> FindByNameAsync(string name)
    {
        return await _productRepository.FindByNameAsync(name);
    }

    public async Task<decimal> CalculateDiscountAsync(List<BasketItemDto> basketItems)
    {
        decimal totalDiscount = 0;

        foreach (var item in basketItems)
        {
            var product = await _dbContext.Products
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

            if (product == null) continue;

            if (product.ProductCategories.Any(pc => pc.Category.Name == "CPU") && item.Quantity >= 2)
            {
                totalDiscount += product.Price * 0.05m;
            }
        }

        return totalDiscount;
    }

    public async Task DeleteProductAsync(int productId)
    {
        await _productRepository.DeleteAsync(productId);
    }

    public async Task ClearDatabaseAsync()
    {
        _dbContext.ProductCategories.RemoveRange(_dbContext.ProductCategories);
        _dbContext.Products.RemoveRange(_dbContext.Products);
        _dbContext.Categories.RemoveRange(_dbContext.Categories);
        await _dbContext.SaveChangesAsync();
        await _dbContext.ResetIdentitySeedAsync();
    }
}