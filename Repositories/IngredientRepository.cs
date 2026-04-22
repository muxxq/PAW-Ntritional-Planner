using Microsoft.EntityFrameworkCore;
using NutriPlan.Data;
using NutriPlan.Models;
using paw_np.Repositories.Interfaces;

namespace paw_np.Repositories
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly NutriPlanDbContext _context;

        public IngredientRepository(NutriPlanDbContext context)
        {
            _context = context;
        }

        public Task<List<Ingredient>> GetAllAsync()
        {
            return _context.Ingredients
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public Task<List<Ingredient>> GetAllByUserAsync(int userId)
        {
            return _context.Ingredients
                .Where(i => i.UserId == userId)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public Task<Ingredient?> GetByIdAsync(int id)
        {
            return _context.Ingredients
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<Ingredient?> GetByIdForUserAsync(int id, int userId)
        {
            return _context.Ingredients
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _context.Ingredients
                .AnyAsync(i => i.Id == id);
        }

        public async Task<Ingredient> CreateAsync(Ingredient entity)
        {
            _context.Ingredients.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Ingredient entity)
        {
            _context.Ingredients.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Ingredient entity)
        {
            _context.Ingredients.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public Task<Ingredient?> GetByNameAsync(string name, int userId)
        {
            return _context.Ingredients
                .FirstOrDefaultAsync(i => i.Name == name && i.UserId == userId);
        }

        public Task<bool> ExistsByNameAsync(string name, int userId, int? excludeId = null)
        {
            return _context.Ingredients.AnyAsync(i =>
                i.UserId == userId &&
                i.Name == name &&
                (!excludeId.HasValue || i.Id != excludeId.Value));
        }
    }
}
