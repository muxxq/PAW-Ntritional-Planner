using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlan.Models
{
    // Un jurnal per zi per utilizator
    public class FoodJournal
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // O singura intrare per zi per user (enforced in DbContext cu index unic)
        public DateOnly JournalDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Suma calculata din toate JournalEntries din ziua respectiva
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalKcalConsumed { get; set; }

        // Navigation
        public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }

    // Fiecare masa/aliment logat intr-o zi
    public class JournalEntry
    {
        public int Id { get; set; }

        public int JournalId { get; set; }
        public FoodJournal FoodJournal { get; set; } = null!;

        // Se poate loga fie o reteta, fie un ingredient direct
        public int? RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int? IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; } = 1;

        // Micul dejun, pranz, cina, gustare
        [Required, MaxLength(50)]
        public string MealType { get; set; } = "Snack";

        // Kcal calculate la momentul logarii
        [Column(TypeName = "decimal(10,2)")]
        public decimal KcalConsumed { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}
