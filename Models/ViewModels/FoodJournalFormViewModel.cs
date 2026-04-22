using System.ComponentModel.DataAnnotations;

namespace paw_np.Models.ViewModels
{
    public class FoodJournalFormViewModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Display(Name = "Data jurnalului")]
        [DataType(DataType.Date)]
        public DateOnly JournalDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Notite")]
        [StringLength(500, ErrorMessage = "Notitele nu pot depasi 500 de caractere.")]
        public string? Notes { get; set; }
    }
}
