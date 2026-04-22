using NutriPlan.Models;

namespace paw_np.Repositories.Interfaces
{
    public interface IRecipeRepository : IRepositoryBase<Recipe>
    {
        Task<List<Recipe>> GetAllByUserAsync(int userId);
        Task<Recipe?> GetByIdForUserAsync(int id, int userId);
        Task<Recipe?> GetByIdWithIngredientsAsync(int id, int userId);

        Task<RecipeIngredient> AddIngredientAsync(RecipeIngredient ingredient);
        Task<RecipeIngredient?> GetRecipeIngredientByIdAsync(int id);
        Task DeleteIngredientAsync(RecipeIngredient ingredient);
    }
}

