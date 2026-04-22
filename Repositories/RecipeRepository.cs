using Microsoft.EntityFrameworkCore;
using NutriPlan.Data;
using NutriPlan.Models;
using paw_np.Repositories.Interfaces;

namespace paw_np.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly NutriPlanDbContext _context;

        public RecipeRepository(NutriPlanDbContext context)
        {
            _context = context;
        }

        public Task<List<Recipe>> GetAllAsync()
        {
            return _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Recipe>> GetAllByUserAsync(int userId)
        {
            return _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<Recipe?> GetByIdAsync(int id)
        {
            return _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
        }

        public Task<Recipe?> GetByIdForUserAsync(int id, int userId)
        {
            return _context.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        }

        public Task<Recipe?> GetByIdWithIngredientsAsync(int id, int userId)
        {
            return _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _context.Recipes.AnyAsync(r => r.Id == id);
        }

        public async Task<Recipe> CreateAsync(Recipe entity)
        {
            _context.Recipes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Recipe entity)
        {
            _context.Recipes.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Recipe entity)
        {
            _context.Recipes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<RecipeIngredient> AddIngredientAsync(RecipeIngredient ingredient)
        {
            _context.RecipeIngredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public Task<RecipeIngredient?> GetRecipeIngredientByIdAsync(int id)
        {
            return _context.RecipeIngredients
                .Include(ri => ri.Recipe)
                .Include(ri => ri.Ingredient)
                .FirstOrDefaultAsync(ri => ri.Id == id);
        }

        public async Task DeleteIngredientAsync(RecipeIngredient ingredient)
        {
            _context.RecipeIngredients.Remove(ingredient);
            await _context.SaveChangesAsync();
        }
    }
}

