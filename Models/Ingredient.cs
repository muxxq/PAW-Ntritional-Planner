using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlan.Models
{
    // Tabelul de ingrediente/produse
    public class Ingredient
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // Valori nutritionale per 100g/ml
        [Column(TypeName = "decimal(8,2)")]
        public decimal CaloriesPer100g { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Proteins { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Carbs { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Fats { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Sugars { get; set; }

        // Unitatea de masura implicita (g, ml, buc)
        [MaxLength(20)]
        public string Unit { get; set; } = "g";

        // Navigation properties
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
        public ICollection<PlannerItem> PlannerItems { get; set; } = new List<PlannerItem>();
        public ICollection<ShoppingListItem> ShoppingListItems { get; set; } = new List<ShoppingListItem>();
    }
}
