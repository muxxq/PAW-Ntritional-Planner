using System.ComponentModel.DataAnnotations;

namespace paw_np.Models.ViewModels
{
    public class FoodJournalEntryFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int JournalId { get; set; }

        [Display(Name = "Recipe ID")]
        public int? RecipeId { get; set; }

        [Display(Name = "Ingredient ID")]
        public int? IngredientId { get; set; }

        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Cantitatea trebuie sa fie mai mare decat 0.")]
        public decimal Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Tipul mesei este obligatoriu.")]
        [StringLength(50, ErrorMessage = "Tipul mesei nu poate depasi 50 de caractere.")]
        [Display(Name = "Tip masa")]
        public string MealType { get; set; } = "Snack";

        [Display(Name = "Kcal consumate")]
        [Range(typeof(decimal), "0", "9999999", ErrorMessage = "Caloriile trebuie sa fie mai mari sau egale cu 0.")]
        public decimal KcalConsumed { get; set; }

        [Display(Name = "Logat la")]
        [DataType(DataType.DateTime)]
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        // View-only properties
        public List<RecipeOptionViewModel> AvailableRecipes { get; set; } = new();
        public List<IngredientOptionViewModel> AvailableIngredients { get; set; } = new();
    }
}
