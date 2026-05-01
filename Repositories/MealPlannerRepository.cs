using Microsoft.EntityFrameworkCore;
using NutriPlan.Data;
using NutriPlan.Models;
using paw_np.Repositories.Interfaces;

namespace paw_np.Repositories
{
    public class MealPlannerRepository : IMealPlannerRepository
    {
        private readonly NutriPlanDbContext _context;

        public MealPlannerRepository(NutriPlanDbContext context)
        {
            _context = context;
        }

        public Task<List<MealPlanner>> GetAllAsync()
        {
            return _context.MealPlanners
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Recipe)
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Ingredient)
                .OrderByDescending(mp => mp.PlanDate)
                .ThenBy(mp => mp.MealType)
                .ToListAsync();
        }

        public Task<List<MealPlanner>> GetAllByUserAsync(int userId)
        {
            return _context.MealPlanners
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Recipe)
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Ingredient)
                .Where(mp => mp.UserId == userId)
                .OrderByDescending(mp => mp.PlanDate)
                .ThenBy(mp => mp.MealType)
                .ToListAsync();
        }

        public Task<MealPlanner?> GetByIdAsync(int id)
        {
            return _context.MealPlanners
                .FirstOrDefaultAsync(mp => mp.Id == id);
        }

        public Task<MealPlanner?> GetByIdForUserAsync(int id, int userId)
        {
            return _context.MealPlanners
                .FirstOrDefaultAsync(mp => mp.Id == id && mp.UserId == userId);
        }

        public Task<MealPlanner?> GetByIdWithItemsAsync(int id, int userId)
        {
            return _context.MealPlanners
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Recipe)
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Ingredient)
                .FirstOrDefaultAsync(mp => mp.Id == id && mp.UserId == userId);
        }

        public Task<MealPlanner?> GetByUserDateMealTypeAsync(int userId, DateOnly planDate, string mealType)
        {
            return _context.MealPlanners
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Recipe)
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Ingredient)
                .FirstOrDefaultAsync(mp => mp.UserId == userId && mp.PlanDate == planDate && mp.MealType == mealType);
        }

        public Task<List<MealPlanner>> GetByUserAndWeekAsync(int userId, DateOnly weekStart)
        {
            var weekEnd = weekStart.AddDays(6);
            return _context.MealPlanners
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Recipe)
                .Include(mp => mp.PlannerItems)
                    .ThenInclude(pi => pi.Ingredient)
                .Where(mp => mp.UserId == userId && mp.PlanDate >= weekStart && mp.PlanDate <= weekEnd)
                .OrderBy(mp => mp.PlanDate)
                .ThenBy(mp => mp.MealType)
                .ToListAsync();
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _context.MealPlanners
                .AnyAsync(mp => mp.Id == id);
        }

        public async Task<MealPlanner> CreateAsync(MealPlanner entity)
        {
            _context.MealPlanners.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(MealPlanner entity)
        {
            _context.MealPlanners.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MealPlanner entity)
        {
            _context.MealPlanners.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public Task<PlannerItem?> GetItemByIdAsync(int itemId, int userId)
        {
            return _context.PlannerItems
                .Include(pi => pi.MealPlanner)
                .Include(pi => pi.Recipe)
                .Include(pi => pi.Ingredient)
                .FirstOrDefaultAsync(pi => pi.Id == itemId && pi.MealPlanner.UserId == userId);
        }

        public async Task<PlannerItem> AddItemAsync(PlannerItem item)
        {
            _context.PlannerItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task UpdateItemAsync(PlannerItem item)
        {
            _context.PlannerItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(PlannerItem item)
        {
            _context.PlannerItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
