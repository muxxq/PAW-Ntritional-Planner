using System.ComponentModel.DataAnnotations;

namespace paw_np.Models.ViewModels
{
    public class ShoppingListFormViewModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Numele listei este obligatoriu.")]
        [StringLength(150, ErrorMessage = "Numele listei nu poate depasi 150 de caractere.")]
        public string Name { get; set; } = string.Empty;
    }
}
