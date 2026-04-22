using NutriPlan.Models;

namespace paw_np.Services.Interfaces
{
    public interface IShoppingListService
    {
        Task<List<ShoppingList>> GetAllAsync(int userId);
        Task<ShoppingList?> GetByIdAsync(int id, int userId);
        Task<(bool Success, string? ErrorMessage, ShoppingList? ShoppingList)> CreateAsync(int userId, ShoppingList shoppingList);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, ShoppingList shoppingList);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId);

        Task<(bool Success, string? ErrorMessage, ShoppingListItem? Item)> AddItemAsync(int userId, int shoppingListId, ShoppingListItem item);
        Task<(bool Success, string? ErrorMessage)> UpdateItemAsync(int userId, int itemId, ShoppingListItem item);
        Task<(bool Success, string? ErrorMessage)> DeleteItemAsync(int userId, int itemId);
        Task<(bool Success, string? ErrorMessage, ShoppingListItem? Item)> ToggleItemCheckedAsync(int userId, int itemId);
    }
}
