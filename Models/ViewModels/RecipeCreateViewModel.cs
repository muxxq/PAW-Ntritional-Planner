using System.ComponentModel.DataAnnotations;

namespace paw_np.Models.ViewModels
{
    public class RecipeCreateViewModel
    {
        // ── Recipe fields ──
        [Required(ErrorMessage = "Numele retetei este obligatoriu.")]
        [MaxLength(200)]
        [Display(Name = "Recipe Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Instructions")]
        public string? Instructions { get; set; }

        [Range(1, 100, ErrorMessage = "Servings trebuie sa fie intre 1 si 100.")]
        public int Servings { get; set; } = 1;

        [Range(0, 1440)]
        [Display(Name = "Prep Time (min)")]
        public int PrepTimeMin { get; set; }

        [Range(0, 1440)]
        [Display(Name = "Cook Time (min)")]
        public int CookTimeMin { get; set; }

        // ── Ingredients to add ──
        // JSON-serialized list of ingredient rows the user built up on the client
        // Each row: { IngredientId, Quantity, Unit }  (IngredientId == 0 means "new ingredient")
        public List<RecipeIngredientRow> Ingredients { get; set; } = new();

        // Dropdown options – populated by controller, never posted
        public List<IngredientOptionViewModel> AvailableIngredients { get; set; } = new();
    }

    /// <summary>
    /// Represents one ingredient row on the Create Recipe form.
    /// If IngredientId > 0 the user picked an existing ingredient.
    /// If IngredientId == 0 the user is creating a brand-new ingredient inline.
    /// </summary>
    public class RecipeIngredientRow
    {
        public int IngredientId { get; set; }

        // New-ingredient fields (used only when IngredientId == 0)
        public string? NewIngredientName { get; set; }
        public decimal NewCaloriesPer100g { get; set; }
        public decimal NewProteins { get; set; }
        public decimal NewCarbs { get; set; }
        public decimal NewFats { get; set; }

        // Quantity used in this recipe
        [Range(typeof(decimal), "0.01", "9999999")]
        public decimal Quantity { get; set; } = 100;

        [MaxLength(20)]
        public string Unit { get; set; } = "g";
    }
}
