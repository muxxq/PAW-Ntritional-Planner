using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlan.Models
{
    // O intrare in calendar (o zi + un tip de masa)
    public class MealPlanner
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Data pentru care este planificata masa
        public DateOnly PlanDate { get; set; }

        // Micul dejun, pranz, cina, gustare
        [Required, MaxLength(50)]
        public string MealType { get; set; } = "Lunch";

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public ICollection<PlannerItem> PlannerItems { get; set; } = new List<PlannerItem>();
    }

    // Ce alimente/retete sunt planificate pentru un slot din calendar
    public class PlannerItem
    {
        public int Id { get; set; }

        public int PlannerId { get; set; }
        public MealPlanner MealPlanner { get; set; } = null!;

        // Fie reteta, fie ingredient direct (ca la JournalEntry)
        public int? RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int? IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; } = 1;
    }
}
