using ComputerStore.Domain.Entities;
using ComputerStore.DTOs;
using ComputerStore.DTOs.DTOs;

namespace ComputerStore.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int productId);
        Task<List<Product>> GetProductsByIdsAsync(List<int> productIds);
        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task<List<DiscountDetailDto>> CalculateDiscountAsync(List<BasketItemDto> basketItems);
        Task ClearDatabaseAsync();
        Task DeleteProductAsync(int productId);
        Task<Product> FindByNameAsync(string name);
    }
}
