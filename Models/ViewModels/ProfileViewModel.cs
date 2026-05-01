using System.ComponentModel.DataAnnotations;

namespace paw_np.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Daily Calorie Target")]
        [Range(0, 99999, ErrorMessage = "Target-ul caloric trebuie sa fie intre 0 si 99999.")]
        public int CaloricTarget { get; set; } = 2000;

        [Display(Name = "Member since")]
        public DateTime CreatedAt { get; set; }

        // Stats
        public int TotalIngredients { get; set; }
        public int TotalRecipes { get; set; }
        public int TotalJournals { get; set; }

        // Optional password change
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [MinLength(6, ErrorMessage = "Parola trebuie sa aiba minim 6 caractere.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Parolele nu coincid.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
