using NutriPlan.Models;
using paw_np.Repositories.Interfaces;
using paw_np.Services.Interfaces;

namespace paw_np.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IIngredientRepository _ingredientRepository;

        public RecipeService(IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository)
        {
            _recipeRepository = recipeRepository;
            _ingredientRepository = ingredientRepository;
        }

        public Task<List<Recipe>> GetAllAsync(int userId)
        {
            return _recipeRepository.GetAllByUserAsync(userId);
        }

        public Task<Recipe?> GetByIdAsync(int id, int userId)
        {
            return _recipeRepository.GetByIdForUserAsync(id, userId);
        }

        public Task<Recipe?> GetByIdWithIngredientsAsync(int id, int userId)
        {
            return _recipeRepository.GetByIdWithIngredientsAsync(id, userId);
        }

        public async Task<(bool Success, string? ErrorMessage, Recipe? Recipe)> CreateAsync(int userId, Recipe recipe)
        {
            if (userId <= 0)
            {
                return (false, "Utilizator invalid.", null);
            }

            recipe.UserId = userId;
            recipe.Name = (recipe.Name ?? string.Empty).Trim();
            recipe.Description = recipe.Description?.Trim();

            if (string.IsNullOrWhiteSpace(recipe.Name))
            {
                return (false, "Numele retetei este obligatoriu.", null);
            }

            recipe.Servings = Math.Max(1, recipe.Servings);
            recipe.PrepTimeMin = Math.Max(0, recipe.PrepTimeMin);
            recipe.CookTimeMin = Math.Max(0, recipe.CookTimeMin);

            if (recipe.CreatedAt == default)
            {
                recipe.CreatedAt = DateTime.UtcNow;
            }

            recipe.TotalKcal = Math.Max(0, recipe.TotalKcal);

            var created = await _recipeRepository.CreateAsync(recipe);
            return (true, null, created);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, Recipe recipe)
        {
            var existing = await _recipeRepository.GetByIdForUserAsync(id, userId);
            if (existing is null)
            {
                return (false, "Reteta nu exista.");
            }

            recipe.Name = (recipe.Name ?? string.Empty).Trim();
            recipe.Description = recipe.Description?.Trim();

            if (string.IsNullOrWhiteSpace(recipe.Name))
            {
                return (false, "Numele retetei este obligatoriu.");
            }

            existing.Name = recipe.Name;
            existing.Description = recipe.Description;
            existing.Servings = Math.Max(1, recipe.Servings);
            existing.PrepTimeMin = Math.Max(0, recipe.PrepTimeMin);
            existing.CookTimeMin = Math.Max(0, recipe.CookTimeMin);

            await _recipeRepository.UpdateAsync(existing);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId)
        {
            var existing = await _recipeRepository.GetByIdForUserAsync(id, userId);
            if (existing is null)
            {
                return (false, "Reteta nu exista.");
            }

            await _recipeRepository.DeleteAsync(existing);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, RecipeIngredient? Ingredient)> AddIngredientAsync(
            int userId,
            int recipeId,
            RecipeIngredient ingredient)
        {
            var recipe = await _recipeRepository.GetByIdWithIngredientsAsync(recipeId, userId);
            if (recipe is null)
            {
                return (false, "Reteta nu exista.", null);
            }

            if (ingredient.Quantity <= 0)
            {
                return (false, "Cantitatea trebuie sa fie mai mare decat 0.", null);
            }

            ingredient.Unit = string.IsNullOrWhiteSpace(ingredient.Unit) ? "g" : ingredient.Unit.Trim();

            // Ingredient must belong to user
            var ownedIngredient = await _ingredientRepository.GetByIdForUserAsync(ingredient.IngredientId, userId);
            if (ownedIngredient is null)
            {
                return (false, "Ingredient invalid.", null);
            }

            if (recipe.RecipeIngredients.Any(ri => ri.IngredientId == ingredient.IngredientId))
            {
                return (false, "Ingredientul exista deja in reteta.", null);
            }

            ingredient.RecipeId = recipeId;
            var created = await _recipeRepository.AddIngredientAsync(ingredient);
            return (true, null, created);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteIngredientAsync(int userId, int recipeIngredientId)
        {
            var recipeIngredient = await _recipeRepository.GetRecipeIngredientByIdAsync(recipeIngredientId);
            if (recipeIngredient is null)
            {
                return (false, "Ingredientul din reteta nu exista.");
            }

            if (recipeIngredient.Recipe.UserId != userId)
            {
                return (false, "Ingredientul din reteta nu exista.");
            }

            await _recipeRepository.DeleteIngredientAsync(recipeIngredient);
            return (true, null);
        }
    }
}

