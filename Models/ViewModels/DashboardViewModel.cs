namespace paw_np.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Daily summary
        public DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public decimal CaloriesConsumed { get; set; }
        public int CaloricTarget { get; set; } = 2000;
        public int CaloriesPercentage => CaloricTarget > 0
            ? (int)Math.Round((double)CaloriesConsumed / CaloricTarget * 100)
            : 0;

        // Macro summary from today's journal entries
        public decimal TotalProteins { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFats { get; set; }

        // Overview counts
        public int TotalIngredients { get; set; }
        public int TotalRecipes { get; set; }
        public int TotalJournals { get; set; }
        public int TotalMealPlans { get; set; }
        public int TotalShoppingLists { get; set; }

        // Recent activity
        public List<RecentJournalViewModel> RecentJournals { get; set; } = new();
        public List<RecentMealPlanViewModel> UpcomingPlans { get; set; } = new();
    }

    public class RecentJournalViewModel
    {
        public int Id { get; set; }
        public DateOnly JournalDate { get; set; }
        public decimal TotalKcalConsumed { get; set; }
        public int EntriesCount { get; set; }
    }

    public class RecentMealPlanViewModel
    {
        public int Id { get; set; }
        public DateOnly PlanDate { get; set; }
        public string MealType { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
    }
}
