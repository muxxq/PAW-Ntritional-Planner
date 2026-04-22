using NutriPlan.Models;
using paw_np.Repositories.Interfaces;
using paw_np.Services.Interfaces;

namespace paw_np.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly IShoppingListRepository _shoppingListRepository;

        public ShoppingListService(IShoppingListRepository shoppingListRepository)
        {
            _shoppingListRepository = shoppingListRepository;
        }

        public Task<List<ShoppingList>> GetAllAsync(int userId)
        {
            return _shoppingListRepository.GetAllByUserAsync(userId);
        }

        public Task<ShoppingList?> GetByIdAsync(int id, int userId)
        {
            return _shoppingListRepository.GetByIdWithItemsAsync(id, userId);
        }

        public async Task<(bool Success, string? ErrorMessage, ShoppingList? ShoppingList)> CreateAsync(int userId, ShoppingList shoppingList)
        {
            shoppingList.UserId = userId;
            shoppingList.Name = (shoppingList.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(shoppingList.Name))
            {
                return (false, "Numele listei este obligatoriu.", null);
            }

            if (shoppingList.UserId <= 0)
            {
                return (false, "Utilizator invalid.", null);
            }

            if (shoppingList.CreatedAt == default)
            {
                shoppingList.CreatedAt = DateTime.UtcNow;
            }

            var createdShoppingList = await _shoppingListRepository.CreateAsync(shoppingList);
            return (true, null, createdShoppingList);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, ShoppingList shoppingList)
        {
            var existingList = await _shoppingListRepository.GetByIdForUserAsync(id, userId);
            if (existingList is null)
            {
                return (false, "Lista de cumparaturi nu exista.");
            }

            shoppingList.Name = (shoppingList.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(shoppingList.Name))
            {
                return (false, "Numele listei este obligatoriu.");
            }

            existingList.Name = shoppingList.Name;
            await _shoppingListRepository.UpdateAsync(existingList);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId)
        {
            var existingList = await _shoppingListRepository.GetByIdForUserAsync(id, userId);
            if (existingList is null)
            {
                return (false, "Lista de cumparaturi nu exista.");
            }

            await _shoppingListRepository.DeleteAsync(existingList);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, ShoppingListItem? Item)> AddItemAsync(int userId, int shoppingListId, ShoppingListItem item)
        {
            var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(shoppingListId, userId);
            if (shoppingList is null)
            {
                return (false, "Lista de cumparaturi nu exista.", null);
            }

            var validationError = ValidateItem(item);
            if (validationError is not null)
            {
                return (false, validationError, null);
            }

            if (IsDuplicateItem(shoppingList.Items, item))
            {
                return (false, "Item-ul exista deja in aceasta lista.", null);
            }

            item.ShoppingListId = shoppingListId;
            item.CustomItemName = NormalizeCustomName(item.CustomItemName);
            item.Unit = NormalizeUnit(item.Unit);

            var createdItem = await _shoppingListRepository.AddItemAsync(item);
            return (true, null, createdItem);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateItemAsync(int userId, int itemId, ShoppingListItem item)
        {
            var existingItem = await _shoppingListRepository.GetItemByIdAsync(itemId, userId);
            if (existingItem is null)
            {
                return (false, "Item-ul nu exista.");
            }

            var validationError = ValidateItem(item);
            if (validationError is not null)
            {
                return (false, validationError);
            }

            var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(existingItem.ShoppingListId, userId);
            if (shoppingList is null)
            {
                return (false, "Lista asociata item-ului nu exista.");
            }

            if (IsDuplicateItem(shoppingList.Items.Where(i => i.Id != itemId), item))
            {
                return (false, "Item-ul exista deja in aceasta lista.");
            }

            existingItem.IngredientId = item.IngredientId;
            existingItem.CustomItemName = NormalizeCustomName(item.CustomItemName);
            existingItem.Quantity = item.Quantity;
            existingItem.Unit = NormalizeUnit(item.Unit);

            await _shoppingListRepository.UpdateItemAsync(existingItem);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteItemAsync(int userId, int itemId)
        {
            var existingItem = await _shoppingListRepository.GetItemByIdAsync(itemId, userId);
            if (existingItem is null)
            {
                return (false, "Item-ul nu exista.");
            }

            await _shoppingListRepository.DeleteItemAsync(existingItem);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, ShoppingListItem? Item)> ToggleItemCheckedAsync(int userId, int itemId)
        {
            var existingItem = await _shoppingListRepository.GetItemByIdAsync(itemId, userId);
            if (existingItem is null)
            {
                return (false, "Item-ul nu exista.", null);
            }

            existingItem.IsChecked = !existingItem.IsChecked;
            await _shoppingListRepository.UpdateItemAsync(existingItem);
            return (true, null, existingItem);
        }

        private static string? ValidateItem(ShoppingListItem item)
        {
            if (item.Quantity <= 0)
            {
                return "Cantitatea trebuie sa fie mai mare decat 0.";
            }

            var customName = NormalizeCustomName(item.CustomItemName);
            var hasIngredient = item.IngredientId.HasValue;
            var hasCustomName = !string.IsNullOrWhiteSpace(customName);

            if (hasIngredient == hasCustomName)
            {
                return "Item-ul trebuie sa aiba fie IngredientId, fie nume custom.";
            }

            return null;
        }

        private static bool IsDuplicateItem(IEnumerable<ShoppingListItem> existingItems, ShoppingListItem incomingItem)
        {
            var incomingCustomName = NormalizeCustomName(incomingItem.CustomItemName);

            return existingItems.Any(existing =>
                (incomingItem.IngredientId.HasValue && existing.IngredientId == incomingItem.IngredientId) ||
                (!string.IsNullOrWhiteSpace(incomingCustomName) &&
                 string.Equals(
                     NormalizeCustomName(existing.CustomItemName),
                     incomingCustomName,
                     StringComparison.OrdinalIgnoreCase)));
        }

        private static string? NormalizeCustomName(string? value)
        {
            var normalized = value?.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }

        private static string NormalizeUnit(string? unit)
        {
            var normalized = unit?.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? "buc" : normalized;
        }
    }
}
