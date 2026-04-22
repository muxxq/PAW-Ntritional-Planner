using Microsoft.EntityFrameworkCore;
using NutriPlan.Data;
using NutriPlan.Models;
using paw_np.Repositories.Interfaces;

namespace paw_np.Repositories
{
    public class ShoppingListRepository : IShoppingListRepository
    {
        private readonly NutriPlanDbContext _context;

        public ShoppingListRepository(NutriPlanDbContext context)
        {
            _context = context;
        }

        public Task<List<ShoppingList>> GetAllAsync()
        {
            return _context.ShoppingLists
                .Include(sl => sl.Items)
                    .ThenInclude(item => item.Ingredient)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public Task<List<ShoppingList>> GetAllByUserAsync(int userId)
        {
            return _context.ShoppingLists
                .Include(sl => sl.Items)
                    .ThenInclude(item => item.Ingredient)
                .Where(sl => sl.UserId == userId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public Task<ShoppingList?> GetByIdAsync(int id)
        {
            return _context.ShoppingLists
                .FirstOrDefaultAsync(sl => sl.Id == id);
        }

        public Task<ShoppingList?> GetByIdForUserAsync(int id, int userId)
        {
            return _context.ShoppingLists
                .FirstOrDefaultAsync(sl => sl.Id == id && sl.UserId == userId);
        }

        public Task<ShoppingList?> GetByIdWithItemsAsync(int id, int userId)
        {
            return _context.ShoppingLists
                .Include(sl => sl.Items)
                    .ThenInclude(item => item.Ingredient)
                .FirstOrDefaultAsync(sl => sl.Id == id && sl.UserId == userId);
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _context.ShoppingLists
                .AnyAsync(sl => sl.Id == id);
        }

        public async Task<ShoppingList> CreateAsync(ShoppingList entity)
        {
            _context.ShoppingLists.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(ShoppingList entity)
        {
            _context.ShoppingLists.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ShoppingList entity)
        {
            _context.ShoppingLists.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public Task<ShoppingListItem?> GetItemByIdAsync(int itemId, int userId)
        {
            return _context.ShoppingListItems
                .Include(item => item.ShoppingList)
                .Include(item => item.Ingredient)
                .FirstOrDefaultAsync(item => item.Id == itemId && item.ShoppingList.UserId == userId);
        }

        public Task<bool> ItemExistsAsync(int itemId)
        {
            return _context.ShoppingListItems
                .AnyAsync(item => item.Id == itemId);
        }

        public async Task<ShoppingListItem> AddItemAsync(ShoppingListItem item)
        {
            _context.ShoppingListItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task UpdateItemAsync(ShoppingListItem item)
        {
            _context.ShoppingListItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(ShoppingListItem item)
        {
            _context.ShoppingListItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
