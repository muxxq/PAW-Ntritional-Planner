namespace paw_np.Models.ViewModels
{
    public class IngredientOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ShoppingListListItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public int CheckedItemCount { get; set; }
    }

    public class ShoppingListItemRowViewModel
    {
        public int Id { get; set; }
        public int ShoppingListId { get; set; }
        public int? IngredientId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "buc";
        public bool IsChecked { get; set; }
    }

    public class ShoppingListIndexViewModel
    {
        public List<ShoppingListListItemViewModel> ShoppingLists { get; set; } = new();
        public ShoppingListFormViewModel CreateForm { get; set; } = new();
    }

    public class ShoppingListDetailsViewModel
    {
        public ShoppingListFormViewModel ShoppingList { get; set; } = new();
        public List<ShoppingListItemRowViewModel> Items { get; set; } = new();
        public ShoppingListItemFormViewModel NewItemForm { get; set; } = new();
    }

    public class ShoppingListDeleteViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }
}
