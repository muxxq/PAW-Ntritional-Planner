using NutriPlan.Models;

namespace paw_np.Repositories.Interfaces
{
    public interface IShoppingListRepository : IRepositoryBase<ShoppingList>
    {
        Task<List<ShoppingList>> GetAllByUserAsync(int userId);
        Task<ShoppingList?> GetByIdForUserAsync(int id, int userId);
        Task<ShoppingList?> GetByIdWithItemsAsync(int id, int userId);

        Task<ShoppingListItem?> GetItemByIdAsync(int itemId, int userId);
        Task<bool> ItemExistsAsync(int itemId);
        Task<ShoppingListItem> AddItemAsync(ShoppingListItem item);
        Task UpdateItemAsync(ShoppingListItem item);
        Task DeleteItemAsync(ShoppingListItem item);
    }
}
