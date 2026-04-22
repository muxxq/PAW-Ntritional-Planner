using NutriPlan.Models;

namespace paw_np.Services.Interfaces
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetAllAsync(int userId);
        Task<Recipe?> GetByIdAsync(int id, int userId);
        Task<Recipe?> GetByIdWithIngredientsAsync(int id, int userId);

        Task<(bool Success, string? ErrorMessage, Recipe? Recipe)> CreateAsync(int userId, Recipe recipe);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, Recipe recipe);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId);

        Task<(bool Success, string? ErrorMessage, RecipeIngredient? Ingredient)> AddIngredientAsync(int userId, int recipeId, RecipeIngredient ingredient);
        Task<(bool Success, string? ErrorMessage)> DeleteIngredientAsync(int userId, int recipeIngredientId);
    }
}

