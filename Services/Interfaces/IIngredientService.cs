using NutriPlan.Models;

namespace paw_np.Services.Interfaces
{
    public interface IIngredientService
    {
        Task<List<Ingredient>> GetAllAsync(int userId);
        Task<Ingredient?> GetByIdAsync(int id, int userId);
        Task<(bool Success, string? ErrorMessage, Ingredient? Ingredient)> CreateAsync(int userId, Ingredient ingredient);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, Ingredient ingredient);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId);
    }
}
