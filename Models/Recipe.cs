using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlan.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        // FK catre User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int Servings { get; set; } = 1;
        public int PrepTimeMin { get; set; }
        public int CookTimeMin { get; set; }

        // Calculat automat din ingrediente
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalKcal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
        public ICollection<PlannerItem> PlannerItems { get; set; } = new List<PlannerItem>();
    }

    // Tabel de legatura Many-to-Many intre Recipe si Ingredient
    public class RecipeIngredient
    {
        public int Id { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;

        // Cantitatea folosita in reteta
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        // Unitatea poate diferi de cea implicita
        [MaxLength(20)]
        public string Unit { get; set; } = "g";
    }
}
