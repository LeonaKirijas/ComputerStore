using ComputerStore.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComputerStore.Application.Interfaces
{
    /// <summary>
    /// Interface for services handling category operations.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Finds a category by its name.
        /// </summary>
        /// <param name="name">The name of the category to find.</param>
        /// <returns>The category if found; otherwise, null.</returns>
        Task<Category> FindByNameAsync(string name);

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="category">The category to create.</param>
        /// <returns>The created category.</returns>
        Task<Category> CreateCategoryAsync(Category category);

        /// <summary>
        /// Optionally, a method to retrieve all categories.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
