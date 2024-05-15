using ComputerStore.DTOs;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using ComputerStore.Infrastructure.Data;

/// <summary>
/// Controller for handling operations related to products.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Constructs a ProductsController with dependency injection for the product service.
    /// </summary>
    /// <param name="productService">Service responsible for product operations.</param>
    public ProductsController(IProductService productService, ICategoryService categoryService, AppDbContext dbContext)
    {
        _productService = productService;
        _categoryService = categoryService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves all available products.
    /// </summary>
    /// <returns>A list of all products.</returns>
    /// <response code="200">Returns the list of all products.</response>
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Retrieves a specific product by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <returns>The requested product if found.</returns>
    /// <response code="200">Product was found and returned successfully.</response>
    /// <response code="404">No product found with the specified ID.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound($"No product found with ID {id}.");
        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="productDto">The product data to create.</param>
    /// <returns>The created product and a URI to the created product.</returns>
    /// <response code="201">Product was created successfully.</response>
    /// <response code="400">Product data is invalid.</response>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newProduct = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Quantity = productDto.Quantity,
            ProductCategories = new List<ProductCategory>()
        };

        foreach (var categoryName in productDto.Categories)
        {
            var existingCategory = await _categoryService.FindByNameAsync(categoryName) ?? await _categoryService.CreateCategoryAsync(new Category { Name = categoryName });
            newProduct.ProductCategories.Add(new ProductCategory { Category = existingCategory });
        }

        await _productService.CreateProductAsync(newProduct);
        return CreatedAtAction(nameof(GetProductById), new { id = newProduct.ProductId }, newProduct);
    }

    /// <summary>
    /// Updates an existing product's information.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="productDto">Updated product data.</param>
    /// <returns>A response indicating the outcome of the operation.</returns>
    /// <response code="204">Product was updated successfully.</response>
    /// <response code="400">Product ID mismatch, or invalid data.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreateDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingProduct = await _productService.GetProductByIdAsync(id);
        if (existingProduct == null)
            return NotFound($"No product found with ID {id}.");

        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Price = productDto.Price;
        existingProduct.Quantity = productDto.Quantity;

        // Update categories
        existingProduct.ProductCategories.Clear();
        foreach (var categoryName in productDto.Categories)
        {
            var existingCategory = await _categoryService.FindByNameAsync(categoryName) ?? await _categoryService.CreateCategoryAsync(new Category { Name = categoryName });
            existingProduct.ProductCategories.Add(new ProductCategory { Category = existingCategory });
        }

        await _productService.UpdateProductAsync(existingProduct);
        return NoContent();
    }

    /// <summary>
    /// Imports a list of products and creates or updates them in the database.
    /// </summary>
    /// <param name="file">The file containing the products to import.</param>
    /// <returns>A confirmation message of successful import.</returns>
    [HttpPost("import")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        List<ProductImportDto> products;
        try
        {
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                var jsonString = await reader.ReadToEndAsync();
                products = JsonConvert.DeserializeObject<List<ProductImportDto>>(jsonString);
            }

            foreach (var productDto in products)
            {
                var existingProduct = await _productService.FindByNameAsync(productDto.Name);
                if (existingProduct == null)
                {
                    var newProduct = new Product
                    {
                        Name = productDto.Name,
                        Price = productDto.Price,
                        Quantity = productDto.Quantity,
                        Description = productDto.Description ?? "Default description",
                        ProductCategories = await MapProductCategories(productDto.Categories)
                    };

                    await _productService.CreateProductAsync(newProduct);
                }
                else
                {
                    existingProduct.Quantity += productDto.Quantity;
                    await _productService.UpdateProductAsync(existingProduct);
                }
            }

            return Ok("Products imported successfully.");
        }
        catch (JsonSerializationException ex)
        {
            return BadRequest($"Error deserializing JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Log the exception (if logging is configured)
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private async Task<ICollection<ProductCategory>> MapProductCategories(List<string> categoryNames)
    {
        var productCategories = new List<ProductCategory>();
        foreach (var name in categoryNames)
        {
            var category = await _categoryService.FindByNameAsync(name) ?? new Category { Name = name, Description = "Default description" };
            productCategories.Add(new ProductCategory { Category = category });
        }
        return productCategories;
    }

    /// <summary>
    /// Calculates discounts for items in the basket.
    /// </summary>
    /// <param name="basketItems">List of items in the customer's basket.</param>
    /// <returns>Total discount amount for the basket.</returns>
    [HttpPost("calculate-discount")]
    public async Task<IActionResult> CalculateDiscount([FromBody] List<BasketItemDto> basketItems)
    {
        var totalDiscount = await _productService.CalculateDiscountAsync(basketItems);
        return Ok(new { totalDiscount });
    }

    [HttpPost("clear-database")]
    public async Task<IActionResult> ClearDatabase()
    {
        await _productService.ClearDatabaseAsync();
        return Ok("Database cleared successfully.");
    }

    /// <summary>
    /// Deletes a product by its identifier.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>A response indicating the outcome of the operation.</returns>
    /// <response code="204">Product was deleted successfully.</response>
    /// <response code="404">No product found with the specified ID.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound($"No product found with ID {id}.");

        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}