using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlan.Models
{
    public class ShoppingList
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Name { get; set; } = "Lista mea de cumparaturi";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
    }

    public class ShoppingListItem
    {
        public int Id { get; set; }

        public int ShoppingListId { get; set; }
        public ShoppingList ShoppingList { get; set; } = null!;

        // Ingredient din repository (optional - poate fi si un item custom)
        public int? IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        // Daca userul adauga ceva care nu exista in Ingredients (ex: "detergent")
        [MaxLength(200)]
        public string? CustomItemName { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; } = 1;

        [MaxLength(20)]
        public string Unit { get; set; } = "buc";

        public bool IsChecked { get; set; } = false;
    }
}
