using NutriPlan.Models;
using paw_np.Repositories.Interfaces;
using paw_np.Services.Interfaces;

namespace paw_np.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientService(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public Task<List<Ingredient>> GetAllAsync(int userId)
        {
            return _ingredientRepository.GetAllByUserAsync(userId);
        }

        public Task<Ingredient?> GetByIdAsync(int id, int userId)
        {
            return _ingredientRepository.GetByIdForUserAsync(id, userId);
        }

        public async Task<(bool Success, string? ErrorMessage, Ingredient? Ingredient)> CreateAsync(int userId, Ingredient ingredient)
        {
            if (userId <= 0)
            {
                return (false, "Utilizator invalid.", null);
            }

            ingredient.Name = ingredient.Name?.Trim() ?? string.Empty;
            ingredient.Unit = NormalizeUnit(ingredient.Unit);
            ingredient.UserId = userId;

            var validationError = ValidateIngredient(ingredient);
            if (validationError is not null)
            {
                return (false, validationError, null);
            }

            var existsByName = await _ingredientRepository.ExistsByNameAsync(ingredient.Name, userId);
            if (existsByName)
            {
                return (false, "Exista deja un ingredient cu acest nume.", null);
            }

            var createdIngredient = await _ingredientRepository.CreateAsync(ingredient);
            return (true, null, createdIngredient);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, Ingredient ingredient)
        {
            var existingIngredient = await _ingredientRepository.GetByIdForUserAsync(id, userId);
            if (existingIngredient is null)
            {
                return (false, "Ingredientul nu exista.");
            }

            ingredient.Name = ingredient.Name?.Trim() ?? string.Empty;
            ingredient.Unit = NormalizeUnit(ingredient.Unit);

            var validationError = ValidateIngredient(ingredient);
            if (validationError is not null)
            {
                return (false, validationError);
            }

            var existsByName = await _ingredientRepository.ExistsByNameAsync(ingredient.Name, userId, id);
            if (existsByName)
            {
                return (false, "Exista deja un ingredient cu acest nume.");
            }

            existingIngredient.Name = ingredient.Name;
            existingIngredient.CaloriesPer100g = ingredient.CaloriesPer100g;
            existingIngredient.Proteins = ingredient.Proteins;
            existingIngredient.Carbs = ingredient.Carbs;
            existingIngredient.Fats = ingredient.Fats;
            existingIngredient.Sugars = ingredient.Sugars;
            existingIngredient.Unit = ingredient.Unit;
            existingIngredient.UserId = userId;

            await _ingredientRepository.UpdateAsync(existingIngredient);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId)
        {
            var existingIngredient = await _ingredientRepository.GetByIdForUserAsync(id, userId);
            if (existingIngredient is null)
            {
                return (false, "Ingredientul nu exista.");
            }

            await _ingredientRepository.DeleteAsync(existingIngredient);
            return (true, null);
        }

        private static string? ValidateIngredient(Ingredient ingredient)
        {
            if (string.IsNullOrWhiteSpace(ingredient.Name))
            {
                return "Numele ingredientului este obligatoriu.";
            }

            if (ingredient.CaloriesPer100g < 0 ||
                ingredient.Proteins < 0 ||
                ingredient.Carbs < 0 ||
                ingredient.Fats < 0 ||
                ingredient.Sugars < 0)
            {
                return "Valorile nutritionale trebuie sa fie mai mari sau egale cu 0.";
            }

            return null;
        }

        private static string NormalizeUnit(string? unit)
        {
            var normalized = unit?.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? "g" : normalized;
        }
    }
}
