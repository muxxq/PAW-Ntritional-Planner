using NutriPlan.Models;

namespace paw_np.Repositories.Interfaces
{
    public interface IMealPlannerRepository : IRepositoryBase<MealPlanner>
    {
        Task<List<MealPlanner>> GetAllByUserAsync(int userId);
        Task<MealPlanner?> GetByIdForUserAsync(int id, int userId);
        Task<MealPlanner?> GetByIdWithItemsAsync(int id, int userId);
        Task<MealPlanner?> GetByUserDateMealTypeAsync(int userId, DateOnly planDate, string mealType);
        Task<List<MealPlanner>> GetByUserAndWeekAsync(int userId, DateOnly weekStart);

        Task<PlannerItem?> GetItemByIdAsync(int itemId, int userId);
        Task<PlannerItem> AddItemAsync(PlannerItem item);
        Task UpdateItemAsync(PlannerItem item);
        Task DeleteItemAsync(PlannerItem item);
    }
}
