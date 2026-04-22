namespace paw_np.Models.ViewModels
{
    public class RecipeOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class FoodJournalListItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly JournalDate { get; set; }
        public string? Notes { get; set; }
        public decimal TotalKcalConsumed { get; set; }
        public int EntriesCount { get; set; }
    }

    public class FoodJournalEntryRowViewModel
    {
        public int Id { get; set; }
        public int JournalId { get; set; }
        public int? RecipeId { get; set; }
        public int? IngredientId { get; set; }
        public string EntryName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string MealType { get; set; } = string.Empty;
        public decimal KcalConsumed { get; set; }
        public DateTime LoggedAt { get; set; }
    }

    public class FoodJournalIndexViewModel
    {
        public List<FoodJournalListItemViewModel> Journals { get; set; } = new();
        public FoodJournalFormViewModel CreateForm { get; set; } = new();
    }

    public class FoodJournalDetailsViewModel
    {
        public FoodJournalFormViewModel Journal { get; set; } = new();
        public decimal TotalKcalConsumed { get; set; }
        public List<FoodJournalEntryRowViewModel> Entries { get; set; } = new();
        public FoodJournalEntryFormViewModel NewEntryForm { get; set; } = new();

        public List<RecipeOptionViewModel> AvailableRecipes { get; set; } = new();
        public List<IngredientOptionViewModel> AvailableIngredients { get; set; } = new();
    }

    public class FoodJournalDeleteViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly JournalDate { get; set; }
        public decimal TotalKcalConsumed { get; set; }
        public int EntriesCount { get; set; }
    }
}
