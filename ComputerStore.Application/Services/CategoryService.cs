using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category> FindByNameAsync(string name)
    {
        return await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        var existingCategory = await FindByNameAsync(category.Name);
        if (existingCategory != null)
        {
            return existingCategory;
        }

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _dbContext.Categories.ToListAsync();
    }
}