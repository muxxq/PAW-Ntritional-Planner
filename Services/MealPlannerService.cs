using NutriPlan.Models;
using paw_np.Repositories.Interfaces;
using paw_np.Services.Interfaces;

namespace paw_np.Services
{
    public class MealPlannerService : IMealPlannerService
    {
        private readonly IMealPlannerRepository _mealPlannerRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IRecipeRepository _recipeRepository;

        public MealPlannerService(
            IMealPlannerRepository mealPlannerRepository,
            IIngredientRepository ingredientRepository,
            IRecipeRepository recipeRepository)
        {
            _mealPlannerRepository = mealPlannerRepository;
            _ingredientRepository = ingredientRepository;
            _recipeRepository = recipeRepository;
        }

        public Task<List<MealPlanner>> GetAllAsync(int userId)
        {
            return _mealPlannerRepository.GetAllByUserAsync(userId);
        }

        public Task<MealPlanner?> GetByIdAsync(int id, int userId)
        {
            return _mealPlannerRepository.GetByIdWithItemsAsync(id, userId);
        }

        public Task<List<MealPlanner>> GetWeekAsync(int userId, DateOnly weekStart)
        {
            return _mealPlannerRepository.GetByUserAndWeekAsync(userId, weekStart);
        }

        public async Task<(bool Success, string? ErrorMessage, MealPlanner? MealPlanner)> CreateAsync(int userId, MealPlanner mealPlanner)
        {
            mealPlanner.UserId = userId;
            mealPlanner.MealType = (mealPlanner.MealType ?? string.Empty).Trim();

            var validationError = ValidatePlanner(mealPlanner);
            if (validationError is not null)
            {
                return (false, validationError, null);
            }

            // Verificam daca exista deja un slot pentru aceeasi zi + tip masa
            var existing = await _mealPlannerRepository.GetByUserDateMealTypeAsync(userId, mealPlanner.PlanDate, mealPlanner.MealType);
            if (existing is not null)
            {
                return (false, "Exista deja un slot pentru aceasta zi si tip de masa.", null);
            }

            var created = await _mealPlannerRepository.CreateAsync(mealPlanner);
            return (true, null, created);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, MealPlanner mealPlanner)
        {
            var existingPlanner = await _mealPlannerRepository.GetByIdForUserAsync(id, userId);
            if (existingPlanner is null)
            {
                return (false, "Planificarea nu exista.");
            }

            mealPlanner.MealType = (mealPlanner.MealType ?? string.Empty).Trim();

            var validationError = ValidatePlanner(mealPlanner);
            if (validationError is not null)
            {
                return (false, validationError);
            }

            // Verificam duplicate (alta intrare cu aceeasi data + tip masa)
            var duplicate = await _mealPlannerRepository.GetByUserDateMealTypeAsync(userId, mealPlanner.PlanDate, mealPlanner.MealType);
            if (duplicate is not null && duplicate.Id != id)
            {
                return (false, "Exista deja un slot pentru aceasta zi si tip de masa.");
            }

            existingPlanner.PlanDate = mealPlanner.PlanDate;
            existingPlanner.MealType = mealPlanner.MealType;
            existingPlanner.Notes = mealPlanner.Notes?.Trim();

            await _mealPlannerRepository.UpdateAsync(existingPlanner);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId)
        {
            var existingPlanner = await _mealPlannerRepository.GetByIdForUserAsync(id, userId);
            if (existingPlanner is null)
            {
                return (false, "Planificarea nu exista.");
            }

            await _mealPlannerRepository.DeleteAsync(existingPlanner);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, PlannerItem? Item)> AddItemAsync(int userId, int plannerId, PlannerItem item)
        {
            var planner = await _mealPlannerRepository.GetByIdWithItemsAsync(plannerId, userId);
            if (planner is null)
            {
                return (false, "Planificarea nu exista.", null);
            }

            var validationError = ValidateItem(item);
            if (validationError is not null)
            {
                return (false, validationError, null);
            }

            // Verificam ca ingredientul/reteta apartine userului
            if (item.IngredientId.HasValue)
            {
                var ingredient = await _ingredientRepository.GetByIdForUserAsync(item.IngredientId.Value, userId);
                if (ingredient is null)
                {
                    return (false, "Ingredientul selectat nu exista.", null);
                }
            }

            if (item.RecipeId.HasValue)
            {
                var recipe = await _recipeRepository.GetByIdForUserAsync(item.RecipeId.Value, userId);
                if (recipe is null)
                {
                    return (false, "Reteta selectata nu exista.", null);
                }
            }

            item.PlannerId = plannerId;
            var created = await _mealPlannerRepository.AddItemAsync(item);
            return (true, null, created);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateItemAsync(int userId, int itemId, PlannerItem item)
        {
            var existingItem = await _mealPlannerRepository.GetItemByIdAsync(itemId, userId);
            if (existingItem is null)
            {
                return (false, "Elementul din planificare nu exista.");
            }

            var validationError = ValidateItem(item);
            if (validationError is not null)
            {
                return (false, validationError);
            }

            existingItem.RecipeId = item.RecipeId;
            existingItem.IngredientId = item.IngredientId;
            existingItem.Quantity = item.Quantity;

            await _mealPlannerRepository.UpdateItemAsync(existingItem);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteItemAsync(int userId, int itemId)
        {
            var existingItem = await _mealPlannerRepository.GetItemByIdAsync(itemId, userId);
            if (existingItem is null)
            {
                return (false, "Elementul din planificare nu exista.");
            }

            await _mealPlannerRepository.DeleteItemAsync(existingItem);
            return (true, null);
        }

        private static string? ValidatePlanner(MealPlanner planner)
        {
            if (planner.UserId <= 0)
            {
                return "Utilizator invalid.";
            }

            if (string.IsNullOrWhiteSpace(planner.MealType))
            {
                return "Tipul mesei este obligatoriu.";
            }

            var validMealTypes = new[] { "Breakfast", "Lunch", "Dinner", "Snack" };
            if (!validMealTypes.Contains(planner.MealType, StringComparer.OrdinalIgnoreCase))
            {
                return "Tipul mesei trebuie sa fie: Breakfast, Lunch, Dinner sau Snack.";
            }

            return null;
        }

        private static string? ValidateItem(PlannerItem item)
        {
            if (item.Quantity <= 0)
            {
                return "Cantitatea trebuie sa fie mai mare decat 0.";
            }

            var hasRecipe = item.RecipeId.HasValue;
            var hasIngredient = item.IngredientId.HasValue;
            if (hasRecipe == hasIngredient)
            {
                return "Elementul trebuie sa aiba fie o reteta, fie un ingredient.";
            }

            return null;
        }
    }
}
