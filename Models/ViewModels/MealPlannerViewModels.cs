using System.ComponentModel.DataAnnotations;

namespace paw_np.Models.ViewModels
{
    public class MealPlannerFormViewModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Display(Name = "Data planificarii")]
        [DataType(DataType.Date)]
        public DateOnly PlanDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Tipul mesei este obligatoriu.")]
        [Display(Name = "Tip masa")]
        public string MealType { get; set; } = "Lunch";

        [Display(Name = "Notite")]
        [StringLength(500, ErrorMessage = "Notitele nu pot depasi 500 de caractere.")]
        public string? Notes { get; set; }
    }

    public class PlannerItemFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int PlannerId { get; set; }

        [Display(Name = "Reteta")]
        public int? RecipeId { get; set; }

        [Display(Name = "Ingredient")]
        public int? IngredientId { get; set; }

        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Cantitatea trebuie sa fie mai mare decat 0.")]
        [Display(Name = "Cantitate")]
        public decimal Quantity { get; set; } = 1;

        public List<RecipeOptionViewModel> AvailableRecipes { get; set; } = new();
        public List<IngredientOptionViewModel> AvailableIngredients { get; set; } = new();
    }

    public class MealPlannerListItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly PlanDate { get; set; }
        public string MealType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int ItemsCount { get; set; }
    }

    public class MealPlannerIndexViewModel
    {
        public List<MealPlannerListItemViewModel> Planners { get; set; } = new();
        public MealPlannerFormViewModel CreateForm { get; set; } = new();
    }

    public class PlannerItemRowViewModel
    {
        public int Id { get; set; }
        public int PlannerId { get; set; }
        public int? RecipeId { get; set; }
        public int? IngredientId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public class MealPlannerDetailsViewModel
    {
        public MealPlannerFormViewModel Planner { get; set; } = new();
        public List<PlannerItemRowViewModel> Items { get; set; } = new();
        public PlannerItemFormViewModel NewItemForm { get; set; } = new();
    }

    public class MealPlannerDeleteViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly PlanDate { get; set; }
        public string MealType { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
    }
}
