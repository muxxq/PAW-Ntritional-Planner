using System.ComponentModel.DataAnnotations;

namespace NutriPlan.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public int CaloricTarget { get; set; } = 2000;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<FoodJournal> FoodJournals { get; set; } = new List<FoodJournal>();
        public ICollection<MealPlanner> MealPlanners { get; set; } = new List<MealPlanner>();
        public ICollection<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
    }
}
