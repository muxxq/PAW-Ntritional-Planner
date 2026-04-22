using System.ComponentModel.DataAnnotations;

namespace paw_np.Models.ViewModels
{
    public class ShoppingListItemFormViewModel
    {
        public int Id { get; set; }
        public int ShoppingListId { get; set; }

        [Display(Name = "Ingredient")]
        public int? IngredientId { get; set; }

        [Display(Name = "Nume custom")]
        [StringLength(200, ErrorMessage = "Numele custom nu poate depasi 200 de caractere.")]
        public string? CustomItemName { get; set; }

        [Range(typeof(decimal), "0.01", "999999", ErrorMessage = "Cantitatea trebuie sa fie mai mare decat 0.")]
        public decimal Quantity { get; set; } = 1;

        [StringLength(20, ErrorMessage = "Unitatea nu poate depasi 20 de caractere.")]
        public string Unit { get; set; } = "buc";

        public bool IsChecked { get; set; }

        public List<IngredientOptionViewModel> AvailableIngredients { get; set; } = new();
    }
}
