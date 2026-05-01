using NutriPlan.Models;

namespace paw_np.Services.Interfaces
{
    public interface IMealPlannerService
    {
        Task<List<MealPlanner>> GetAllAsync(int userId);
        Task<MealPlanner?> GetByIdAsync(int id, int userId);
        Task<List<MealPlanner>> GetWeekAsync(int userId, DateOnly weekStart);

        Task<(bool Success, string? ErrorMessage, MealPlanner? MealPlanner)> CreateAsync(int userId, MealPlanner mealPlanner);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, MealPlanner mealPlanner);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId);

        Task<(bool Success, string? ErrorMessage, PlannerItem? Item)> AddItemAsync(int userId, int plannerId, PlannerItem item);
        Task<(bool Success, string? ErrorMessage)> UpdateItemAsync(int userId, int itemId, PlannerItem item);
        Task<(bool Success, string? ErrorMessage)> DeleteItemAsync(int userId, int itemId);
    }
}
